# BitrixJiraConnector.Api

REST API service tự động đồng bộ **Bitrix24 CRM → Jira**, chạy như Windows Service.  
Đây là phiên bản tái cấu trúc của `BitrixJiraConnector` (WinForms), giữ nguyên nghiệp vụ nhưng thay UI bằng REST API.

---

## Tính năng

- Tự động quét Bitrix24 định kỳ, lấy danh sách Won/Lost Deal mới
- Tạo Jira Issue tương ứng với từng loại Deal (6 loại)
- Ghi Jira key và URL ngược lại vào Bitrix Deal
- Gửi email thông báo (thành công / lỗi) qua SendGrid
- Cơ chế email leo thang 3 lần khi deal có lỗi
- Lưu trạng thái xử lý và log lỗi vào SQLite
- REST API để tra cứu, trigger thủ công, chỉnh cấu hình

---

## Tech Stack

| | |
|---|---|
| Runtime | .NET 10 |
| Framework | ASP.NET Core Web API |
| Database | SQLite + Entity Framework Core 9 |
| Jira client | Atlassian.SDK 13 |
| Email | SendGrid 9 |
| JSON | Newtonsoft.Json 13 |
| OpenAPI | Built-in (`Microsoft.AspNetCore.OpenApi`) |

---

## Yêu cầu

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Truy cập được Bitrix24 REST API
- Truy cập được Jira (basic auth)
- SendGrid API key

---

## Cài đặt & Chạy

### 1. Clone và restore

```bash
git clone <repo-url>
cd BitrixJiraConnector.Api
dotnet restore
```

### 2. Cấu hình

Sao chép và chỉnh sửa `appsettings.json` (xem [Cấu hình](#cấu-hình) bên dưới).

### 3. Migrate database

```bash
dotnet ef database update
```

### 4. Chạy development

```bash
dotnet run
```

OpenAPI schema: `http://localhost:<port>/openapi/v1.json`

---

## Cài đặt Windows Service

```bash
# Build release
dotnet publish -c Release -o C:\BitrixJiraConnector\Api

# Cài service (chạy với quyền Administrator)
sc create BitrixJiraApi binPath="C:\BitrixJiraConnector\Api\BitrixJiraConnector.Api.exe" start=auto
sc start BitrixJiraApi

# Gỡ service
sc stop BitrixJiraApi
sc delete BitrixJiraApi
```

---

## Cấu hình

Tất cả cấu hình nằm trong `appsettings.json`:

```json
{
  "Bitrix": {
    "ApiUrl": "https://<domain>/rest/<userId>/<token>",
    "BaseUrl": "https://<domain>",
    "DealDetailUrl": "https://<domain>/crm/deal/details",
    "User": "connector",
    "Password": "<password>",
    "AttachFilePath": "C:\\BitrixClient\\AttachFile\\",
    "LogPath": "C:\\BitrixClient\\Logs\\"
  },
  "Jira": {
    "Url": "https://jira.<domain>",
    "User": "connector",
    "Password": "<password>"
  },
  "Email": {
    "SendGridApiKey": "<api-key>",
    "FromAddress": "<from@email.com>",
    "FromName": "Bitrix Jira Connector",
    "FallbackToAddress": "<admin@email.com>",
    "RenewalManagerEmail": "<renewal-manager@email.com>",
    "SalesManagerEmail": "<sales-manager@email.com>",
    "AdminBitrixEmail": "<bitrix-admin@email.com>",
    "AdminJiraEmail": "<jira-admin@email.com>"
  },
  "Scanning": {
    "IntervalMinutes": 5,
    "LookbackDays": 1,
    "ResendEmailAfterHours": 50
  },
  "ConnectionStrings": {
    "Default": "Data Source=ConnectBitrixAndJira.db"
  }
}
```

Cấu hình `IntervalMinutes` và `ResendEmailAfterHours` có thể thay đổi runtime qua API `/api/config` mà không cần restart service.

---

## API

### Deals

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/deals` | Tìm kiếm deal |
| `GET` | `/api/deals/{dealId}` | Lấy thông tin 1 deal |
| `POST` | `/api/deals/{dealId}/process` | Xử lý thủ công 1 deal |

**Query params GET `/api/deals`:**

| Param | Type | Mặc định | Mô tả |
|---|---|---|---|
| `dealId` | int? | — | Lọc theo Deal ID |
| `fromDate` | DateTime? | 30 ngày trước | Từ ngày |
| `toDate` | DateTime? | Hôm nay | Đến ngày |

**Response chung:**

```json
{
  "success": true,
  "data": { ... },
  "message": ""
}
```

### Config

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/config` | Lấy tất cả cấu hình |
| `PUT` | `/api/config` | Cập nhật cấu hình |

**Body PUT `/api/config`:**

```json
{
  "quetLaiSau": 5,
  "guiLaiEmailSau": 50,
  "soNgayQuet": 1
}
```

### Health

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/health` | Kiểm tra service |

```json
{
  "success": true,
  "data": {
    "status": "running",
    "startedAt": "2024-01-01T08:00:00",
    "applicationStopping": false
  }
}
```

---

## Luồng xử lý Deal

```
Background Scan (mỗi N phút)
    │
    ├─ Lấy danh sách Deal ID từ Bitrix (Won/Lost trong N ngày gần nhất)
    │
    └─ Với mỗi Deal:
         ├─ Đã tạo Jira rồi? → Bỏ qua
         ├─ Deal vừa sửa < 3 phút? → Bỏ qua (file chưa upload xong)
         ├─ Lost deal + không phải NgungHuyDichVu? → Bỏ qua
         ├─ Thiếu field bắt buộc? → Ghi lỗi DB + Email leo thang
         └─ Đủ điều kiện:
              ├─ Tạo Jira Issue
              ├─ Ghi Jira key/URL ngược lại Bitrix
              └─ Gửi email thông báo thành công
```

### 6 Loại Deal

| Loại | ID | Jira Project | Issue Type | Summary |
|---|---|---|---|---|
| Triển khai mới | 3095 | TRIENKHAI | Epic | `Triển Khai - {SanPham} - {TenKhachSan}` |
| Triển khai bổ sung | 3096 | TRIENKHAI | Epic | `{YeuCauThem} - {TenKhachSan}` |
| Ngừng/Hủy dịch vụ | 3097 | TRIENKHAI | HDNgungHuyDichVu | `Ngừng/Hủy dịch vụ - {TenKhachSan}` |
| Chuyển đổi dịch vụ | 3098 | TRIENKHAI | Epic | `Chuyển đổi - {SanPham} - {TenKhachSan}` |
| Cấp key | 3099 | ES | ServiceRequest | `{TenDeal}` |
| Hỗ trợ khác | 3100 | ES | ServiceRequest | `{TenDeal}` |

### Email leo thang khi lỗi

| Lần | Thời điểm gửi | Người nhận |
|---|---|---|
| 1 | Ngay lập tức | Người phụ trách deal |
| 2 | Sau 17h cùng ngày hoặc 9h sáng hôm sau | Người phụ trách deal |
| 3 | 9h sáng ngày sau lần 2 | Sales/Renewal Manager |

Sau lần 3: deal bị bỏ qua, không xử lý thêm.

---

## Database Migrations

```bash
# Tạo migration mới
dotnet ef migrations add <TenMigration>

# Apply migration
dotnet ef database update

# Xem danh sách migrations
dotnet ef migrations list

# Rollback về migration cụ thể
dotnet ef database update <TenMigration>
```

---

## Cấu trúc thư mục

```
BitrixJiraConnector.Api/
├── Configurations/        # Settings POCO, hằng số Jira/Bitrix, enum
├── Models/
│   ├── Database/          # EF entities + DbContext + DesignTimeFactory
│   ├── Bitrix/            # Model dữ liệu từ Bitrix API
│   └── Dto/               # Request/Response DTOs
├── Services/
│   ├── Interfaces/        # Contracts
│   ├── BitrixService.cs   # Gọi Bitrix API
│   ├── JiraService.cs     # Tạo Jira Issue
│   ├── DbService.cs       # CRUD SQLite
│   ├── DealProcessingService.cs  # Điều phối xử lý deal
│   ├── EmailService.cs    # Gửi email SendGrid
│   └── DealLockService.cs # Per-deal lock tránh race condition
├── BackgroundServices/    # Hosted service quét định kỳ
├── Controllers/           # REST endpoints
├── Helpers/               # Utility: validate field, parse Bitrix JSON
├── Migrations/            # EF Core migrations
├── Program.cs
└── appsettings.json
```
