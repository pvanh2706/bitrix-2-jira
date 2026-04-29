# BitrixJiraConnector.Api

> **Quy tắc bắt buộc**: Mỗi khi thay đổi code trong project này (thêm endpoint, sửa service, thêm field, đổi logic...), **phải cập nhật file CLAUDE.md này** để phản ánh đúng trạng thái hiện tại. AI sẽ đọc file này thay vì đọc lại toàn bộ code.

---

## Tổng quan

ASP.NET Core Web API (.NET 10) — phiên bản tái cấu trúc của `BitrixJiraConnector` (WinForms .NET 7).  
Chức năng giống hệt bản cũ: tự động đồng bộ Bitrix24 CRM → Jira, chạy như Windows Service.

**Điểm khác so với bản WinForms:**
- Không có UI — giao tiếp qua REST API
- Dependency Injection đúng chuẩn (Scoped/Singleton thay vì static)
- Interface-driven (dễ test, dễ mock)
- OpenAPI built-in (không dùng Swashbuckle — không tương thích .NET 10)

---

## Stack

| Thành phần | Phiên bản |
|---|---|
| .NET | 10.0 |
| ASP.NET Core | 10.0 |
| Entity Framework Core + SQLite | 9.0.5 |
| Atlassian.SDK (Jira client) | 13.0.0 |
| Newtonsoft.Json | 13.0.3 |
| SendGrid | 9.29.3 |
| OpenAPI | built-in (`Microsoft.AspNetCore.OpenApi`) |

---

## Cấu trúc thư mục

```
BitrixJiraConnector.Api/
├── Configurations/
│   ├── AppSettings.cs            # POCO settings: BitrixSettings, JiraSettings, EmailSettings, ScanningSettings
│   ├── ConfigJiraBitrix.cs       # Hằng số: deal type IDs, Jira project/issue-type IDs, required fields, field name map
│   └── EnumDataDefine.cs         # Enum: LOAI_DEAL, SAVE_TIME_SEND_MAIL_TO, TYPE_PIPE_LINE
├── Models/
│   ├── Database/
│   │   ├── AppDbContext.cs        # EF Core DbContext
│   │   ├── AppDbContextFactory.cs # Design-time factory cho EF migrations
│   │   ├── BitrixJiraInfo.cs      # Entity: trạng thái xử lý mỗi deal
│   │   ├── ConfigData.cs          # Entity: cấu hình key-value
│   │   └── ExceptionLog.cs        # Entity: log lỗi theo dealId
│   ├── Bitrix/
│   │   ├── BitrixDataDeal.cs      # Model deal đã parse (dùng nội bộ)
│   │   ├── BitrixDataDealApiResult.cs
│   │   ├── UserBitrix.cs
│   │   ├── ContactBitrix.cs
│   │   ├── FileHopDongAttach.cs
│   │   └── DataProductInDeal.cs
│   └── Dto/
│       ├── ApiResponse.cs         # Wrapper response chung: { success, data, message }
│       ├── DealSummaryDto.cs      # Response GET /api/deals
│       ├── DealSearchRequest.cs   # Query params tìm kiếm deal
│       ├── ProcessDealResult.cs   # Response POST /api/deals/{id}/process
│       ├── SaveConfigRequest.cs   # Body PUT /api/config
│       └── CheckSendEmail.cs      # Internal DTO kiểm tra email retry
├── Services/
│   ├── Interfaces/
│   │   ├── IBitrixService.cs
│   │   ├── IJiraService.cs
│   │   ├── IDbService.cs
│   │   ├── IDealProcessingService.cs
│   │   ├── IEmailService.cs
│   │   └── IDealLockService.cs
│   ├── BitrixService.cs           # Gọi Bitrix API, parse deal, lấy user/contact
│   ├── JiraService.cs             # Tạo Jira issue theo loại deal, đính kèm file
│   ├── DbService.cs               # CRUD SQLite: BitrixJiraInfo, ConfigData, ExceptionLog
│   ├── DealProcessingService.cs   # Điều phối xử lý deal: validate → tạo Jira → ghi Bitrix → email
│   ├── EmailService.cs            # Gửi email qua SendGrid
│   └── DealLockService.cs         # Per-deal SemaphoreSlim để tránh xử lý đồng thời cùng 1 deal
├── BackgroundServices/
│   └── DealScanBackgroundService.cs  # IHostedService: quét deal định kỳ
├── Controllers/
│   ├── DealsController.cs         # GET/POST /api/deals
│   ├── ConfigController.cs        # GET/PUT /api/config
│   └── HealthController.cs        # GET /api/health
├── Helpers/
│   ├── CheckRequireFieldBitrix.cs # Kiểm tra field bắt buộc trong JObject deal
│   └── BitrixConvertData.cs       # Parse custom field từ JSON Bitrix
├── Migrations/                    # EF Core migrations
├── Program.cs                     # Entry point, DI registration
└── appsettings.json               # Config: Bitrix, Jira, Email, Scanning, ConnectionStrings
```

---

## DI Registration (Program.cs)

| Service | Lifetime | Ghi chú |
|---|---|---|
| `AppDbContext` | Scoped | SQLite via EF Core |
| `IDealLockService` → `DealLockService` | **Singleton** | ConcurrentDictionary<dealId, SemaphoreSlim> |
| `IDbService` → `DbService` | Scoped | |
| `IEmailService` → `EmailService` | Scoped | |
| `IBitrixService` → `BitrixService` | Scoped | Dùng named HttpClient "Bitrix" |
| `IJiraService` → `JiraService` | Scoped | |
| `IDealProcessingService` → `DealProcessingService` | Scoped | |
| `DealScanBackgroundService` | Hosted (Singleton) | Tạo scope mới mỗi vòng lặp |

**Startup**: Auto-migrate DB khi khởi động (`db.Database.Migrate()`).  
**Windows Service**: `builder.Host.UseWindowsService()`.  
**OpenAPI**: chỉ bật ở Development (`app.MapOpenApi()` → `/openapi/v1.json`).

---

## API Endpoints

### `DealsController` — `/api/deals`

| Method | Path | Mô tả |
|---|---|---|
| `GET` | `/api/deals` | Tìm kiếm deal theo `dealId`, `fromDate`, `toDate` (query params). Mặc định 30 ngày gần nhất. |
| `GET` | `/api/deals/{dealId}` | Lấy thông tin 1 deal theo ID. |
| `POST` | `/api/deals/{dealId}/process` | Xử lý thủ công 1 deal (bỏ qua background scan). |

**Response**: `ApiResponse<T>` — `{ success: bool, data: T, message: string }`

### `ConfigController` — `/api/config`

| Method | Path | Mô tả |
|---|---|---|
| `GET` | `/api/config` | Lấy tất cả cấu hình từ bảng `ConfigData`. |
| `PUT` | `/api/config` | Lưu cấu hình: `quetLaiSau` (phút), `guiLaiEmailSau` (giờ), `soNgayQuet` (ngày). |

### `HealthController` — `/api/health`

| Method | Path | Mô tả |
|---|---|---|
| `GET` | `/api/health` | Kiểm tra service còn sống, trả về `status`, `startedAt`, `applicationStopping`. |

---

## Business Logic

### Luồng chính (Background scan)

`DealScanBackgroundService` → mỗi N phút (đọc từ DB) → `DealProcessingService.ScanAndProcessAllDealsAsync`:

1. Lấy danh sách deal ID từ Bitrix API (Won/Lost trong N ngày gần nhất)
2. Với mỗi deal:
   - Acquire per-deal lock (`DealLockService`)
   - Kiểm tra trong DB: đã xử lý thành công chưa? (`IsSendDataToJira == 1`)
   - Bỏ qua nếu deal vừa chỉnh sửa < 3 phút (tránh file chưa upload xong)
   - Bỏ qua nếu Lost deal mà không phải loại `NgungHuyDichVu`
   - Validate trường bắt buộc theo loại deal
   - Nếu thiếu field → ghi lỗi DB → gửi email leo thang
   - Nếu đủ → Tạo Jira issue → Ghi Jira key/URL ngược lại Bitrix → Gửi email thành công

### 6 Loại Deal → Jira Issue

| Loại Deal | ID Bitrix | Jira Project | Issue Type | Summary |
|---|---|---|---|---|
| TrienKhaiMoi | 3095 | TRIENKHAI | Epic (10000) | `Triển Khai - {SanPham} - {TenKhachSan}` |
| TrienKhaiBoSung | 3096 | TRIENKHAI | Epic (10000) | `{YeuCauThem} - {TenKhachSan}` |
| NgungHuyDichVu | 3097 | TRIENKHAI | HDNgungHuyDichVu (11600) | `Ngừng/Hủy dịch vụ - {TenKhachSan}` |
| ChuyenDoiDichVu | 3098 | TRIENKHAI | Epic (10000) | `Chuyển đổi - {SanPham} - {TenKhachSan}` |
| CapKey | 3099 | ES | ServiceRequest (10301) | `{TenDeal}` |
| HoTroKhac | 3100 | ES | ServiceRequest (10301) | `{TenDeal}` |

### Email Leo Thang (lỗi)

| Lần | Điều kiện | Người nhận | Ghi DB |
|---|---|---|---|
| 1 | Ngay khi phát hiện lỗi | Responsible (người phụ trách deal) | `DateTimeSendMailFirst` |
| 2 | Sau 17h cùng ngày hoặc 9h sáng hôm sau | Responsible | `DateTimeSendMailSecond` |
| 3 | 9h sáng ngày sau lần 2 | Sales/Renewal Manager + CC Admin | `DateTimeSendMailThird` |

Sau lần 3 → bỏ qua deal (không xử lý thêm).

---

## Database Schema (SQLite)

### `BitrixJiraInfoes`
| Column | Type | Ghi chú |
|---|---|---|
| `Bitrix_DealID` | int (PK) | ID deal Bitrix |
| `Bitrix_DealLink` | string | URL deal trên Bitrix |
| `Bitrix_DateSearch` | string | Ngày quét |
| `IsSendDataToJira` | int | 1 = đã tạo Jira thành công |
| `IsSendEmail` | int | |
| `Jira_Link` | string | URL issue Jira |
| `HaveError` | int | 1 = có lỗi |
| `ErrorInfo` | string | Mô tả lỗi |
| `DateTimeCreated` | string | |
| `NumberCheckError` | int | Số lần thử lại |
| `DateTimeSendMailFirst` | long? | Unix timestamp |
| `DateTimeSendMailSecond` | long? | Unix timestamp |
| `DateTimeSendMailThird` | long? | Unix timestamp |
| `LastChangeData` | string | |

### `ConfigData`
| Column | Type |
|---|---|
| `ID` | int (PK, auto) |
| `KeyConfig` | string |
| `ValueConfig` | string |
| `Description` | string |

**Config keys**: `QuetLaiSau` (phút quét), `GuiLaiEmailSau` (giờ), `SoNgayQuet` (số ngày lookback).

### `ExceptionLog`
| Column | Type |
|---|---|
| `Id` | int (PK, auto) |
| `DealID` | int |
| `ExceptionMessage` | string |
| `StackTrace` | string |
| `ExceptionType` | string |
| `Source` | string |
| `LoggedAt` | string |

---

## Configuration (appsettings.json)

```json
{
  "Bitrix": {
    "ApiUrl": "...",           // Endpoint REST Bitrix (bao gồm token)
    "BaseUrl": "...",          // Base URL Bitrix
    "DealDetailUrl": "...",    // URL xem deal (dùng tạo link)
    "User": "connector",
    "Password": "...",
    "AttachFilePath": "...",   // Thư mục lưu file hợp đồng tải về
    "LogPath": "..."
  },
  "Jira": {
    "Url": "https://jira.ezcloudhotel.com",
    "User": "connector",
    "Password": "..."
  },
  "Email": {
    "SendGridApiKey": "...",
    "FromAddress": "...",
    "FromName": "...",
    "FallbackToAddress": "...",   // CC mặc định (admin)
    "RenewalManagerEmail": "...", // Email leo thang lần 3 (Renewal pipeline)
    "SalesManagerEmail": "...",   // Email leo thang lần 3 (Sale pipeline)
    "AdminBitrixEmail": "...",
    "AdminJiraEmail": "..."
  },
  "Scanning": {
    "IntervalMinutes": 5,         // Chu kỳ quét (có thể override từ DB)
    "LookbackDays": 1,
    "ResendEmailAfterHours": 50
  },
  "ConnectionStrings": {
    "Default": "Data Source=ConnectBitrixAndJira.db"
  }
}
```

---

## Bitrix Field IDs quan trọng

| Field ID | Tên hiển thị |
|---|---|
| `UF_CRM_1616066206` | Jira URL — **dấu hiệu deal đã tạo issue** |
| `UF_CRM_1616066175` | Jira Key |
| `UF_CRM_1713881390` | Loại Deal |
| `UF_CRM_5B3F32B1118E0` | Tên khách sạn |
| `UF_CRM_5D98084D1476E` | Nhu cầu sản phẩm |
| `UF_CRM_1613788692724` | Hợp đồng & Phụ lục (file đính kèm) |
| `UF_CRM_1708697569` | Thị trường |

---

## Quy tắc đặc biệt

- **Deal < 3 phút**: bỏ qua, đánh dấu `HaveGetLate = true` (tránh file chưa xong upload)
- **Lost deal**: chỉ xử lý khi loại là `NgungHuyDichVu` (3097), các loại khác bỏ qua
- **File hợp đồng > 10MB**: bỏ qua đính kèm, không báo lỗi
- **Per-deal lock**: dùng `SemaphoreSlim` để tránh race condition khi background scan và API trigger cùng xử lý 1 deal

---

## Migrations

```bash
# Chạy trong thư mục BitrixJiraConnector.Api/
dotnet ef migrations add <TenMigration>
dotnet ef database update
dotnet ef migrations list
```

`AppDbContextFactory` tự động đọc `appsettings.json` để cung cấp connection string khi chạy EF design-time tools.
