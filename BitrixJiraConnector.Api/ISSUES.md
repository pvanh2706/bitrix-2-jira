# Backend Issues & Improvement Checklist

> Generated: 2026-05-01

---

## 🔴 CRITICAL – Phải sửa ngay

- [x] **Hardcoded deal ID = 49453** (`BitrixService.cs` ~L45) — đã bỏ filter `ID=49453`, uncomment `>=DATE_MODIFY`
- [x] **Không có authentication/authorization** — đã thêm `ApiKeyMiddleware` (header `X-Api-Key`); key cấu hình qua `ApiKey` trong `appsettings.json` hoặc env var; `/api/health` được bỏ qua
- [x] **Race condition: duplicate Jira issue** (`DealProcessingService.cs` ~L161) — đã ghi DB ngay sau `CreateIssueAsync` (trước `PostJiraDataToDealAsync`); `PostJiraDataToDealAsync` được wrap try-catch, lỗi chỉ log không re-throw
- [ ] **Credentials lưu plain text** trong `appsettings.json` (Bitrix password, Jira password, SendGrid API key) — cần dùng environment variables hoặc Secret Manager
- [x] **Hardcoded Jira URL** `https://jira.ezcloudhotel.com` trong `DealProcessingService.cs` ~L192 — đã dùng `SystemConfig["jira_url"]` từ DB

---

## 🟠 HIGH – Nên sửa sớm

- [x] **Hardcoded email domain** `@ezcloud.vn` trong `GetJiraUsernameByEmailAsync` (`JiraService.cs` ~L308) — đã dùng `SystemConfig["email_domain_strip"]` từ DB
- [ ] **Không có retry logic** cho Bitrix/Jira API (`BitrixService.cs` ~L545) — lỗi mạng thoáng qua là fail ngay, cần exponential backoff
- [ ] **Không có pagination** trên endpoint `/api/deals` (`DealsController.cs` ~L35) — query 30 ngày deal có thể timeout
- [ ] **Kiểm tra kích thước file sau khi download** (`JiraService.cs` ~L292) — 10MB check xảy ra sau khi download xong, lãng phí băng thông, cần dùng HEAD request trước
- [ ] **File download thất bại không xóa file rác** (`BitrixService.cs` ~L430) — tích lũy garbage trên disk
- [ ] **N+1 API calls khi lấy contacts** (`BitrixService.cs` ~L117) — mỗi contact gọi `GetContactByIdAsync()` riêng, cần batch
- [ ] **Không xử lý lỗi `PostJiraDataToDealAsync`** (`DealProcessingService.cs` ~L155) — nếu fail, deal bị treo ở trạng thái không nhất quán

---

## 🟡 MEDIUM – Cải thiện trong sprint này

- [ ] **DealLockService memory leak** (`DealLockService.cs` ~L11) — `ConcurrentDictionary` giữ semaphore mãi mãi, không có cleanup
- [ ] **DryRun mode không hoàn chỉnh** (`DealProcessingService.cs` ~L87) — vẫn ghi DB và gửi email dù là dry run
- [ ] **`Bitrix_DateSearch` lưu dạng string** thay vì `DateTime` — so sánh ngày bằng `string.Compare()` trong `DbService.cs` ~L47, dễ sai
- [ ] **Database path tương đối** `ConnectBitrixAndJira.db` trong `appsettings.json` — chạy Windows Service sẽ tìm từ System32
- [ ] **Thiếu logging** cho các API call quan trọng (`BitrixService.cs`) — khó debug khi API Bitrix có vấn đề
- [ ] **`/health` endpoint quá đơn giản** (`HealthController.cs`) — chỉ trả start time, không check DB hay external API health
- [ ] **SemaphoreSlim timeout 30s hardcoded** (`DealProcessingService.cs` ~L80) — nên configurable
- [ ] **Magic numbers** Jira issue type ID (`"10000"`, `"11600"`, `"10301"`) rải khắp code — cần đưa vào enum/constants
- [ ] **Không có database indexes** trên `Bitrix_DealID` và cột tìm kiếm — query chậm khi bảng lớn
- [ ] **`DateTime.Parse()` không có try-catch** (`BitrixService.cs` ~L260) — malformed date từ Bitrix API sẽ crash
- [ ] **Bare `catch` block** trong `GetJiraUsernameByEmailAsync` (`JiraService.cs` ~L312) — nuốt lỗi im lặng, không log
- [ ] **Không validate URL từ config** (`BitrixService.cs` ~L32, `JiraService.cs`) — typo gây lỗi runtime khó debug
- [ ] **Không seed required config data** trong migrations — sau deploy phải setup DB thủ công
- [ ] **Deal status hardcoded string** `"S"`, `"F"` — nên dùng enum có ý nghĩa

---

## 🔵 LOW – Nice to have

- [ ] **Không có webhook support** — chỉ có polling qua background service, không phản ứng real-time với Bitrix
- [ ] **Không có audit trail** cho config changes (`ConfigController.cs`) — không biết ai thay đổi gì lúc nào
- [ ] **Không có soft delete** — record bị xóa mất hoàn toàn, không thể recover
- [ ] **Không có integration tests** — thay đổi logic deal processing không có safety net
- [ ] **Email logic phụ thuộc system time** (`DealProcessingService.cs` ~L220) — khó viết unit test
- [ ] **Không có OpenAPI docs** cho DryRun mode (`Program.cs` ~L34)
- [ ] **Inconsistent null checks** — mix giữa `!= null`, `?.`, `??` trong cùng codebase
- [ ] **Deep nesting** trong `BuildDealResultAsync` (`BitrixService.cs` ~L260) — nên extract thành các method nhỏ hơn

---

## Ghi chú

- File này theo dõi tất cả vấn đề phát hiện qua code review ngày 2026-05-01
- Đánh dấu `[x]` khi đã fix xong
