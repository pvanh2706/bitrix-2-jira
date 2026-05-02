# Cơ chế xử lý đồng thời (Concurrency) — Deal Processing

## Tổng quan

Hệ thống có **2 nguồn** có thể kích hoạt xử lý cùng 1 deal:
1. **Background scan** — `DealScanBackgroundService` quét định kỳ theo `IntervalMinutes`
2. **API thủ công** — `POST /api/deals/{dealId}/process`

---

## Cơ chế bảo vệ: DealLockService

```csharp
// DealLockService (Singleton)
private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();
public SemaphoreSlim GetLock(int dealId) => _locks.GetOrAdd(dealId, _ => new SemaphoreSlim(1, 1));
```

- Mỗi `dealId` có 1 `SemaphoreSlim(1,1)` riêng — chỉ cho 1 luồng vào xử lý tại một thời điểm
- `DealLockService` là **Singleton** → lock tồn tại suốt vòng đời app, dùng chung cho mọi scope/request
- Timeout: **30 giây** — nếu chờ quá 30s không lấy được lock → trả về lỗi, không treo mãi

```csharp
bool acquired = await sem.WaitAsync(TimeSpan.FromSeconds(30), token);
if (!acquired)
    return new ProcessDealResult { Success = false, Message = "Timeout waiting for deal lock" };
```

---

## Cơ chế bảo vệ 2: alreadyCreated check (Bitrix API)

Ngay đầu `ProcessDealInternalAsync`, sau khi lấy thông tin deal từ Bitrix:

```csharp
bool alreadyCreated = dataDealApi.UF_CRM_1616066206 != null 
                   && (string)dataDealApi.UF_CRM_1616066206 != "";
```

`UF_CRM_1616066206` là field link Jira được ghi vào Bitrix sau khi tạo issue thành công. Nếu đã có → bỏ qua.

---

## Các kịch bản và kết quả

### Kịch bản 1: Background đang xử lý, API gọi vào

```
Background                        API /deals/X/process
──────────────────────────────    ──────────────────────
GetLock(X) → WaitAsync → OK
ProcessDealInternalAsync()
  → CreateIssueAsync()             GetLock(X) → WaitAsync
  → PostJiraDataToDealAsync()          (chờ tối đa 30s)
Release()                         → OK, vào ProcessDealInternalAsync()
                                  → GetDealByIdAsync(X)
                                  → UF_CRM_1616066206 đã có link
                                  → "Deal đã tạo iss" → bỏ qua ✓
```
**Kết quả: Issue chỉ tạo 1 lần.**

---

### Kịch bản 2: API gọi vào trước, Background quét được deal

Hoàn toàn đối xứng với kịch bản 1, kết quả tương tự.

---

### Kịch bản 3: API gọi 2 lần nhanh liên tiếp (double-click)

```
Request 1               Request 2
──────────────────────  ──────────────────────
GetLock(X) → OK
Processing...           GetLock(X) → CHỜ (max 30s)
Release()               → OK → alreadyCreated → bỏ qua ✓
```
**Kết quả: Issue chỉ tạo 1 lần.**

---

### Kịch bản 4: Xử lý deal X mất > 30 giây, request 2 timeout lock

```
Request 1               Request 2
──────────────────────  ──────────────────────
GetLock(X) → OK
Processing (> 30s)...   GetLock(X) → chờ 30s
                        → Timeout → trả về lỗi 
                        { Success: false, Message: "Timeout waiting for deal lock" }
Release()
```
**Kết quả: Request 2 thất bại với lỗi timeout. Issue không tạo trùng.**  
Request 2 cần retry sau khi request 1 hoàn thành.

---

## Lỗi có thể xảy ra: Tạo issue trùng (race condition hiếm)

**Điều kiện xảy ra:**
1. `CreateIssueAsync()` thành công → issue đã tạo trên Jira
2. `PostJiraDataToDealAsync()` gọi Bitrix để ghi link Jira → **mạng timeout / Bitrix lỗi**
3. Link Jira **chưa được ghi** vào `UF_CRM_1616066206` trên Bitrix
4. Lần quét tiếp theo: check `alreadyCreated` vẫn thấy rỗng → **tạo issue lần 2**

```
Lần 1:
  CreateIssueAsync()         → Jira issue A-123 ✓
  PostJiraDataToDealAsync()  → TIMEOUT ✗ (link chưa ghi vào Bitrix)
  
Lần 2 (sau IntervalMinutes):
  check alreadyCreated       → rỗng (vì lần 1 không ghi được)
  CreateIssueAsync()         → Jira issue A-124 ✗ (trùng!)
```

**Xác suất:** Rất thấp, chỉ xảy ra khi Bitrix API có sự cố đúng lúc ghi link.

**Cách phát hiện:** Kiểm tra bảng `ExceptionLog` hoặc log lỗi có `PostJiraDataToDealAsync`.

**Cách giảm thiểu (nếu muốn xử lý triệt để):**
- Lưu `jiraKey` vào bảng `BitrixJiraInfo` **trước** khi gọi `PostJiraDataToDealAsync`
- Khi xử lý deal, kiểm tra cả bảng local DB xem deal đã có `Jira_Link` chưa
- Nếu có trong DB nhưng chưa có trên Bitrix → retry ghi lên Bitrix thay vì tạo issue mới

---

## Tóm tắt

| Bảo vệ | Cơ chế | Phạm vi |
|---|---|---|
| **DealLockService** | `SemaphoreSlim(1,1)` per dealId, timeout 30s | In-process, single instance |
| **alreadyCreated check** | Đọc `UF_CRM_1616066206` từ Bitrix API | Cross-process, cross-restart |
| **Điểm yếu** | `CreateIssue` OK nhưng `PostJiraData` fail | Race condition hiếm gặp |
