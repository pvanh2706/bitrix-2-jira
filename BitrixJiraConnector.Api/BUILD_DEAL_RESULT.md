# Luồng xử lý `BuildDealResultAsync`

**File:** `Services/BitrixService.cs`  
**Mục đích:** Nhận raw JSON của một deal từ Bitrix API, phân tích và map sang `BitrixDataDealApiResult`. Quyết định xem deal có đủ điều kiện tạo Jira issue hay không.

---

## Đầu vào / Đầu ra

| | Type | Mô tả |
|---|---|---|
| **Input** | `dynamic dataDealApi` | JSON của deal lấy từ `crm.deal.get` |
| **Input** | `JObject customFields` | Định nghĩa các custom field của Bitrix (`crm.deal.fields`) |
| **Output** | `BitrixDataDealApiResult` | Kết quả phân tích, gồm cờ trạng thái và data đã map |

### Các cờ trong `BitrixDataDealApiResult`

| Cờ | Ý nghĩa khi `true` |
|---|---|
| `HaveGetLate` | Deal vừa được sửa đổi trong vòng 3 phút — **bỏ qua**, thử lại sau |
| `HaveCreateIssues` | Deal **không cần** tạo issue mới (đã tạo rồi, loại deal không hỗ trợ...) |
| `HaveError` | Deal có lỗi — thiếu trường bắt buộc — **gửi email thông báo** |
| (không có cờ nào) | Deal hợp lệ, đủ điều kiện → **tiến hành tạo Jira issue** |

---

## Sơ đồ luồng

```
Nhận dataDealApi + customFields
         │
         ▼
┌─────────────────────────────────────────────────┐
│  1. Kiểm tra DATE_MODIFY                        │
│     Nếu deal được sửa < 3 phút trước            │
│     → HaveGetLate = true → RETURN (bỏ qua)      │
└─────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│  2. Kiểm tra deal đã tạo issue chưa             │
│     UF_CRM_1616066206 (Jira URL) != ""          │
│     → HaveCreateIssues = true                   │
│     → Message = "Deal đã tạo iss" → RETURN      │
└─────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│  3. Kiểm tra deal bị lost không hợp lệ          │
│     STAGE_SEMANTIC_ID == "F" (Lost)             │
│     VÀ LoaiDeal != NgungHuyDichVu (3097)        │
│     → HaveCreateIssues = true                   │
│     → Message = "Lost deal không nằm trong      │
│       danh mục tạo iss" → RETURN                │
└─────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│  4. Kiểm tra LoaiDeal có được chọn chưa         │
│     UF_CRM_1713881390 == ""                     │
│     → HaveError = true                          │
│     → Message = HTML lỗi "không chọn Loại Deal" │
│     → RETURN                                    │
└─────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│  5. Xác định danh sách trường bắt buộc          │
│     theo LoaiDeal:                              │
│                                                 │
│  3095 - Triển khai mới    → FieldRequire_TKMoi  │
│  3096 - Triển khai bổ sung→ FieldRequire_TKBoSung│
│  3097 - Ngừng/hủy DV      → FieldRequire_NgungHuy│
│  3098 - Chuyển đổi DV     → FieldRequire_ChuyenDoi│
│  3099 - Cấp key           → FieldRequire_CapKey │
│  3100 - Hỗ trợ khác       → FieldRequire_HoTroKhac│
│  (khác) → dealNotNeedCreateIss = true           │
└─────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│  6. Lấy thông tin Responsible user              │
│     API: crm.user.get?id=ASSIGNED_BY_ID         │
│     → Responsible_Email, FirstName, LastName    │
└─────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│  7. Lấy thông tin liên hệ triển khai (Contact)  │
│     API: crm.deal.contact.list?id=dealId        │
│     → Duyệt từng Contact, gọi crm.contact.get  │
│     → Tìm contact có UF_CRM_1715045713 == "1"  │
│       (Là thông tin liên hệ khi triển khai)     │
│     → Lấy: Tên, Chức vụ, SĐT, Email            │
│                                                 │
│     Nếu thiếu Tên/SĐT/Email → thêm vào         │
│     missingFields                               │
└─────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│  8. Kiểm tra dealNotNeedCreateIss               │
│     (LoaiDeal không thuộc 3095–3100)            │
│     → HaveCreateIssues = true                   │
│     → Message = "Deal không nằm trong loại tạo  │
│       iss" → RETURN                             │
└─────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│  9. Kiểm tra missingFields                      │
│     Nếu có trường còn thiếu                     │
│     → HaveError = true                          │
│     → Message = HTML lỗi liệt kê tên trường    │
│     → ToAddressEmail = Responsible_Email        │
│     → RETURN                                    │
└─────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│  10. Map tất cả trường vào BitrixDataDeal        │
│      (Xem chi tiết mục dưới)                    │
│                                                 │
│      Đặc biệt: Nếu LoaiDeal = CapKey (3099)    │
│      → Gọi API lấy Section của từng sản phẩm   │
│        để replace PRODUCT_NAME bằng tên Section │
└─────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│  11. Tải file đính kèm (Hợp đồng & Phụ lục)    │
│      UF_CRM_1613788692724                       │
│      → Download từng file về server             │
│      → Lưu path vào HopDong_PhuLuc             │
└─────────────────────────────────────────────────┘
         │
         ▼
   Trả về result.DataDeal đã đầy đủ
   → Caller sẽ tiến hành tạo Jira issue
```

---

## Chi tiết các điều kiện thoát sớm (early return)

### Bước 1 — `HaveGetLate`
```
dataDealApi.DATE_MODIFY + 3 phút >= DateTime.Now
```
Deal vừa được người dùng sửa trên Bitrix, dữ liệu có thể chưa ổn định. Hệ thống bỏ qua lần này, lần scan tiếp theo sẽ xử lý lại.

---

### Bước 2 — `HaveCreateIssues` (đã tạo)
```
dataDealApi.UF_CRM_1616066206 != null && != ""
```
Field `UF_CRM_1616066206` là Jira URL được ghi ngược lại vào Bitrix sau khi tạo issue thành công. Nếu field này đã có giá trị → deal đã được xử lý → không tạo lại.

---

### Bước 3 — `HaveCreateIssues` (lost không hợp lệ)
```
STAGE_SEMANTIC_ID == "F"  AND  LoaiDeal != "3097"
```
Deal ở trạng thái Lost (`"F"`) chỉ được tạo issue nếu là loại **Ngừng/Hủy dịch vụ** (3097). Các loại lost khác không xử lý.

---

### Bước 4 — `HaveError` (chưa chọn loại deal)
```
UF_CRM_1713881390 == ""
```
Loại Deal chưa được chọn, không xác định được quy trình xử lý → lỗi.

---

### Bước 8 — `HaveCreateIssues` (loại deal không hỗ trợ)
LoaiDeal không thuộc 6 loại được định nghĩa → deal này không nằm trong phạm vi tự động tạo issue.

---

### Bước 9 — `HaveError` (thiếu trường bắt buộc)
Danh sách trường bắt buộc theo từng loại deal, cộng thêm 3 trường từ Contact:

| Key nội bộ | Tên hiển thị |
|---|---|
| `THONTINLIENHE_TENKHACH` | Tên khách hàng (liên hệ triển khai) |
| `THONTINLIENHE_SDT` | Số điện thoại (liên hệ triển khai) |
| `THONTINLIENHE_EMAIL` | Email (liên hệ triển khai) |

---

## Mapping trường theo loại deal

### Trường chung (tất cả loại deal)

| Field Bitrix | Property C# | Mô tả |
|---|---|---|
| `ID` | `DealID` | ID deal |
| `TITLE` | `TenDeal` | Tên deal |
| `UF_CRM_1713881390` | `LoaiDeal` | Loại deal (enum 3095–3100) |
| `CATEGORY_ID` | `Pipeline` | Pipeline (Sale/Renewal/Cross-sale) |
| `ASSIGNED_BY_ID` | `Responsible_*` | Nhân viên phụ trách |
| `UF_CRM_5B3F32B1118E0` | `TenKhachSan` | Tên khách sạn |
| `UF_CRM_1613727861184` | `MaKhachSan` | Mã khách sạn |
| `COMPANY_ID` | `CompanyId` | ID công ty |
| `SOURCE_ID` | `Source` | Nguồn deal |
| `UF_CRM_5D984E2CA62C9` | `DC_Khach_SoNhaDuong` | Địa chỉ: Số nhà, đường |
| `UF_CRM_5D984E38B7502` | `DC_Khach_TinhThanhPho` | Địa chỉ: Tỉnh/thành phố |
| `UF_CRM_5D984E34ACFB6` | `DC_Khach_QuanHuyen` | Địa chỉ: Quận/huyện |
| `UF_CRM_5D984E30EEE34` | `DC_Khach_PhuongXa` | Địa chỉ: Phường/xã |
| `UF_CRM_5B3F32B1068C7` | `SoPhong` | Số phòng |
| `UF_CRM_6054492BF24D8` | `SoGiuong` | Số giường |
| `UF_CRM_5D98084D1476E` | `NhuCauSanPham` | Nhu cầu sản phẩm (array → join ",") |
| `UF_CRM_1708697569` | `ThiTruong` | Thị trường |
| `UF_CRM_1616123841` | `LoaiHinhKhachSan` | Loại hình khách sạn |
| `UF_CRM_1708697773` | `ChinhSachGia` | Chính sách giá |
| `UF_CRM_1708698013` | `MayChuChayPhanMem` | Máy chủ chạy phần mềm |
| `UF_CRM_1715996452` | `DungThuHayDungThat` | Dùng thử hay dùng thật |
| `UF_CRM_1616123465` | `TrangThaiThanhToanLan1` | Trạng thái thanh toán lần 1 |
| `UF_CRM_1614068621890` | `NoiDungChuyenKhoanLan1` | Nội dung chuyển khoản lần 1 |
| `UF_CRM_1616130516` | `PhuongThucTrienKhai` | Phương thức triển khai |
| `UF_CRM_1616130611` | `ThoiDiemTrienKhai` | Thời điểm triển khai |
| `UF_CRM_1616130767` | `TaikhoanDungThuPMS` | Tài khoản dùng thử PMS |
| `UF_CRM_1616130823` | `Neu_TK_BE_CoTichHop_ezFolioKhong` | Tích hợp ezFolio? ("1" → "Có") |
| `UF_CRM_1616130900` | `ThongTinTrienKhaiWeb` | Thông tin triển khai web |
| `UF_CRM_1531008138` | `GhiChu` | Ghi chú (lấy phần tử đầu tiên) |
| `UF_CRM_1708698698` | `YeuCauThem` | Yêu cầu thêm |
| `UF_CRM_1613788692724` | `HopDong_PhuLuc` | File hợp đồng & phụ lục |

### Trường riêng cho Ngừng/Hủy (3097)

| Field Bitrix | Property C# | Mô tả |
|---|---|---|
| `UF_CRM_1570243307` | `LyDoLost` | Lý do lost (array → list string) |
| `UF_CRM_1570243354` | `GhichuChoLyDoLost` | Ghi chú lý do lost |
| `UF_CRM_1708699318` | `LoaiYeuCauHuy` | Loại yêu cầu hủy |
| `UF_CRM_1715997025` | `ThoiDiemNgungHuy` | Thời điểm ngừng/hủy |

### Trường riêng cho Cấp Key (3099)

| Field Bitrix | Property C# | Mô tả |
|---|---|---|
| `UF_CRM_1600922068` | `NgayBatDauTinh_GHBT` | Ngày bắt đầu tính giảm hạn bản thử |
| `UF_CRM_1600922103` | `NgayKetThucHan_GHBT` | Ngày kết thúc hạn bản thử |
| `UF_CRM_1715997550` | `TinhHuongCanCapKey` | Tình huống cần cấp key |
| `DanhSachSanPham` | `DanhSachSanPham` | Sản phẩm → **tra Section để lấy tên nhóm** |

---

## Xử lý sản phẩm cho Cấp Key

Đây là bước phức tạp nhất, thực hiện khi `LoaiDeal == "3099"` và có sản phẩm:

```
Với mỗi sản phẩm trong DanhSachSanPham:
  → crm.product.get?id=PRODUCT_ID         (lấy SECTION_ID)
  → crm.product.section.list             (tra tên Section)
  → Thay PRODUCT_NAME bằng tên Section
```

**Lý do:** Tên sản phẩm trong deal thường là SKU cụ thể, trong khi Jira issue cần tên nhóm sản phẩm (Section) để phân loại đúng.

---

## Xử lý file đính kèm

```
UF_CRM_1613788692724: [ { id, showUrl, downloadUrl }, ... ]
```

Với mỗi file:
1. Download về server qua HTTP (Basic Auth: Bitrix User/Password)
2. Lưu vào `AttachFilePath` (cấu hình trong `appsettings`)
3. Ghi lại `file_Name` và `path_file` để đính kèm vào Jira issue

---

## Tóm tắt các API Bitrix được gọi

| Thứ tự | API | Mục đích |
|---|---|---|
| (trước khi gọi hàm) | `crm.deal.get` | Lấy raw data deal |
| (trước khi gọi hàm) | `crm.deal.fields` | Lấy định nghĩa custom fields |
| Bước 6 | `crm.user.get` | Lấy thông tin Responsible |
| Bước 7 | `crm.deal.contact.list` | Lấy danh sách contact của deal |
| Bước 7 | `crm.contact.get` (×N) | Lấy chi tiết từng contact |
| Bước 10 | `crm.deal.productrows.get` | Lấy sản phẩm trong deal |
| Bước 10* | `crm.product.get` (×N) | *(Chỉ CapKey)* Lấy SECTION_ID |
| Bước 10* | `crm.product.section.list` | *(Chỉ CapKey)* Lấy tên Section |
| Bước 11 | HTTP Download | Tải file hợp đồng về server |
