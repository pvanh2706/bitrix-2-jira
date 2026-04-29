# Migration Plan: WinForms .NET 7 → ASP.NET Core Web API .NET 10

## Vấn đề hiện tại cần xử lý

| Bug/Vấn đề | File | Nguy cơ |
|---|---|---|
| `AppDbContext` static singleton | `BitrixJiraDBServices.cs` | Race condition, data corruption khi concurrent |
| `new HttpClient()` mỗi lần gọi | `BitrixServices.cs` | Socket exhaustion, memory leak |
| `using System.Windows.Forms` | `JiraServices.cs` | Build fail trên non-Windows |
| `throw ex` thay vì `throw` | 12+ chỗ trong code | Mất stack trace khi debug |
| `dealIDProcessed` static List | `ConfigJiraBitrix.cs` | Mất sau restart, không thread-safe |

---

## Cấu trúc thư mục sau khi chuyển đổi

```
BitrixJiraConnector.Api/
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── Configurations/
│   ├── AppSettings.cs                  # Strongly-typed options: BitrixSettings, JiraSettings, EmailSettings
│   ├── ConfigJiraBitrix.cs             # Giữ lại chỉ const: field IDs, deal type IDs, required field lists
│   ├── ApiBitrixConstants.cs           # Đổi tên từ ApiBitrix.cs
│   └── EnumDataDefine.cs               # Giữ nguyên
├── Models/
│   ├── Database/                       # Tách từ DatabaseModels.cs
│   │   ├── AppDbContext.cs
│   │   ├── BitrixJiraInfo.cs
│   │   ├── ConfigData.cs
│   │   └── ExceptionLog.cs
│   ├── Bitrix/                         # Tách từ BitrixModels.cs
│   │   ├── BitrixDataDeal.cs
│   │   ├── BitrixDataDealApiResult.cs
│   │   ├── UserBitrix.cs
│   │   ├── ContactBitrix.cs
│   │   ├── FileHopDongAttach.cs
│   │   └── DataProductInDeal.cs
│   └── Dto/
│       ├── CreateIssueRequest.cs
│       ├── DealSearchRequest.cs
│       ├── DealSummaryDto.cs
│       ├── SaveConfigRequest.cs
│       └── ApiResponse.cs              # Wrapper: { success, data, message }
├── Services/
│   ├── Interfaces/
│   │   ├── IBitrixService.cs
│   │   ├── IJiraService.cs
│   │   ├── IDealProcessingService.cs
│   │   ├── IDealLockService.cs
│   │   ├── IDbService.cs
│   │   └── IEmailService.cs
│   ├── BitrixService.cs                # Refactor từ BitrixServices.cs
│   ├── JiraService.cs                  # Refactor từ JiraServices.cs
│   ├── DealProcessingService.cs        # Mới: tách logic điều phối
│   ├── DealLockService.cs              # Mới: chống trùng lặp
│   ├── DbService.cs                    # Refactor từ BitrixJiraDBServices.cs
│   └── EmailService.cs                 # Refactor từ SendEmail.cs
├── BackgroundServices/
│   └── DealScanBackgroundService.cs    # Thay vòng lặp trong FormMain
├── Controllers/
│   ├── DealsController.cs
│   ├── ConfigController.cs
│   └── HealthController.cs
└── Migrations/                         # Giữ nguyên, không cần tạo lại
```

---

## API Endpoints

| Method | Endpoint | Mô tả |
|---|---|---|
| `POST` | `/api/deals/{dealId}/process` | Tạo issue thủ công từ dealID |
| `GET` | `/api/deals?fromDate=&toDate=&dealId=` | Danh sách deal đã xử lý |
| `GET` | `/api/deals/{dealId}` | Chi tiết 1 deal |
| `GET` | `/api/config` | Đọc cấu hình từ DB |
| `PUT` | `/api/config` | Lưu cấu hình |
| `GET` | `/api/health` | Trạng thái background service |

---

## Cơ chế chống trùng lặp

Dùng `ConcurrentDictionary<int, SemaphoreSlim>` — mỗi dealID có 1 semaphore riêng.
Lý do dùng `SemaphoreSlim` thay vì `lock`: `lock` không hỗ trợ `async/await` bên trong.

```
Auto-scan ──┐
            ├──► await sem.WaitAsync(30s) ──► check DB ──► tạo issue ──► sem.Release()
API call ───┘
```

Người gọi thứ 2 chờ tối đa 30 giây. Khi được vào, check DB thấy deal đã xử lý → return sớm, không tạo duplicate.

```csharp
// DealLockService.cs — đăng ký Singleton
public class DealLockService : IDealLockService
{
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

    public SemaphoreSlim GetLock(int dealId)
        => _locks.GetOrAdd(dealId, _ => new SemaphoreSlim(1, 1));
}

// DealProcessingService.cs — dùng lock
var sem = _lockService.GetLock(dealId);
await sem.WaitAsync(TimeSpan.FromSeconds(30), token);
try
{
    // check DB → tạo issue → lưu kết quả
}
finally
{
    sem.Release();
}
```

---

## appsettings.json

```json
{
  "Bitrix": {
    "ApiUrl": "https://worktest.ezcloudhotel.com/rest/861/mgna8op38m1k5vcx",
    "BaseUrl": "https://worktest.ezcloudhotel.com",
    "DealDetailUrl": "https://work.ezcloudhotel.com/crm/deal/details",
    "User": "connector",
    "Password": "Batdau@2023",
    "AttachFilePath": "C:\\BitrixClient\\AttachFile\\",
    "LogPath": "C:\\BitrixClient\\Logs\\"
  },
  "Jira": {
    "Url": "https://jira.ezcloudhotel.com",
    "User": "connector",
    "Password": "Batdau@2024"
  },
  "Email": {
    "SendGridApiKey": "SG...",
    "FromAddress": "bitrix-jira.noreply@gmail.com",
    "FromName": "Bitrix Jira Connector",
    "FallbackToAddress": "anh.pham@ezcloud.vn",
    "RenewalManagerEmail": "sale3.hn@ezcloud.vn",
    "SalesManagerEmail": "huyenlt@ezcloud.vn",
    "AdminJiraEmail": "dung.nguyen@ezcloud.vn"
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

---

## Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService(); // Tự detect Windows Service vs console

// Options
builder.Services.Configure<BitrixSettings>(builder.Configuration.GetSection("Bitrix"));
builder.Services.Configure<JiraSettings>(builder.Configuration.GetSection("Jira"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// Database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// HttpClient
builder.Services.AddHttpClient("Bitrix");

// Services
builder.Services.AddSingleton<IDealLockService, DealLockService>();
builder.Services.AddScoped<IDbService, DbService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IBitrixService, BitrixService>();
builder.Services.AddScoped<IJiraService, JiraService>();
builder.Services.AddScoped<IDealProcessingService, DealProcessingService>();

// Background service
builder.Services.AddHostedService<DealScanBackgroundService>();

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
```

---

## DealScanBackgroundService

```csharp
public class DealScanBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbService = scope.ServiceProvider.GetRequiredService<IDbService>();
            var processingService = scope.ServiceProvider.GetRequiredService<IDealProcessingService>();

            // Đọc interval từ DB mỗi cycle để nhận config mới nhất
            int intervalMinutes = await dbService.GetScanIntervalMinutesAsync();

            await processingService.ScanAndProcessAllDealsAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }
}
```

> **Lưu ý:** `DealScanBackgroundService` đăng ký `Singleton`. Phải dùng `IServiceScopeFactory`
> để tạo scope trước khi resolve `IDealProcessingService` (Scoped). Không inject Scoped service
> trực tiếp vào Singleton — sẽ bị lỗi runtime.

---

## 15 Bước thực hiện

> **Thứ tự quan trọng:** Bước 2 (Options Pattern) phải làm trước tất cả vì mọi service phụ thuộc vào config.
> Bước 4 (DealLockService) phải làm trước Bước 9 (DealProcessingService).

### Bước 1 — Tạo project ASP.NET Core mới

- Tạo `BitrixJiraConnector.Api.csproj` với `<TargetFramework>net10.0</TargetFramework>`
- Xóa `<OutputType>WinExe</OutputType>` và `<UseWindowsForms>true</UseWindowsForms>`
- Thêm packages: `Microsoft.EntityFrameworkCore.Sqlite` (9.x), `Atlassian.SDK` (13.x), `Newtonsoft.Json` (13.x), `SendGrid` (9.x), `Microsoft.Extensions.Hosting.WindowsServices`
- Xóa `FormMain.cs`, `FormMain.Designer.cs`, `FormMain.resx`, `Properties/` folder

### Bước 2 — Chuyển config sang Options Pattern

- Tạo `Configurations/AppSettings.cs` với 3 nested classes: `BitrixSettings`, `JiraSettings`, `EmailSettings`
- Tạo `appsettings.json` với toàn bộ giá trị đang hard-code trong `ConfigJiraBitrix.cs`
- Sửa `ConfigJiraBitrix.cs`: xóa toàn bộ static mutable fields (`quetSauPhut`, `soNgayQuet`, `dealIDProcessed`, `haveReloadConfig`, credentials, URLs), chỉ giữ lại `const` và `static readonly` (field IDs, deal type IDs, required field lists, `keyValueField_Bitrix`)

### Bước 3 — Refactor AppDbContext

- Xóa `OnConfiguring()` override hard-code connection string
- Thêm constructor `AppDbContext(DbContextOptions<AppDbContext> options) : base(options)`
- Tách 3 entity ra file riêng trong `Models/Database/`

### Bước 4 — Tạo DealLockService

- Tạo `Services/Interfaces/IDealLockService.cs`
- Tạo `Services/DealLockService.cs` dùng `ConcurrentDictionary<int, SemaphoreSlim>`
- Đăng ký `Singleton` trong `Program.cs`

### Bước 5 — Refactor DbService

- Tạo `Services/Interfaces/IDbService.cs` và `Services/DbService.cs`
- Xóa `private static readonly AppDbContext _dbContext` singleton — đây là bug nghiêm trọng
- Inject `AppDbContext` qua constructor (Scoped lifetime)
- Chuyển `SaveChanges()` → `SaveChangesAsync()` toàn bộ
- Thêm method `GetScanIntervalMinutesAsync()` cho BackgroundService đọc interval

### Bước 6 — Refactor EmailService

- Tạo `Services/Interfaces/IEmailService.cs` và `Services/EmailService.cs`
- Inject `IOptions<EmailSettings>` thay vì dùng `ConfigJiraBitrix` constants
- Xóa method SMTP cũ không dùng nữa
- Đăng ký `Scoped`

### Bước 7 — Refactor BitrixService

- Tạo `Services/Interfaces/IBitrixService.cs` và `Services/BitrixService.cs`
- Inject `IHttpClientFactory` (named `"Bitrix"`) thay vì `new HttpClient()`
- Inject `IOptions<BitrixSettings>`, `IDbService`, `IEmailService`
- Xóa method `getDeals_Bitrix()` (sẽ chuyển vào `DealProcessingService`)
- Sửa `FileHopDongAttach.downloadFile()`: nhận credentials và path qua config thay vì hard-code
- Sửa tất cả `throw ex` → `throw`
- Đăng ký `Scoped`

### Bước 8 — Refactor JiraService

- Tạo `Services/Interfaces/IJiraService.cs` và `Services/JiraService.cs`
- **Xóa `using System.Windows.Forms`** — lý do chính không thể build trên non-Windows
- Xóa `using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database`
- Inject `IOptions<JiraSettings>`, `IDbService`, `IEmailService`, `ILogger<JiraService>`
- Sửa `productNameCheck` từ instance field thành local variable (thread-safety khi concurrent)
- Thay `File.AppendAllText(pathLog, ...)` trong `setFileAttach_Jira` bằng `ILogger`
- Sửa tất cả `throw ex` → `throw`
- Đăng ký `Scoped`

### Bước 9 — Tạo DealProcessingService

- Tạo `Services/Interfaces/IDealProcessingService.cs`:
  ```csharp
  Task ScanAndProcessAllDealsAsync(CancellationToken token);
  Task<ProcessDealResult> ProcessSingleDealAsync(int dealId, CancellationToken token);
  ```
- Tạo `Services/DealProcessingService.cs`: chứa logic từ `getDataDealBitrix_And_CreateIssues` và `getDeals_Bitrix`, tích hợp `IDealLockService`
- Inject `IBitrixService`, `IJiraService`, `IDbService`, `IEmailService`, `IDealLockService`
- Đăng ký `Scoped`

### Bước 10 — Tạo DealScanBackgroundService

- Tạo `BackgroundServices/DealScanBackgroundService.cs` implement `BackgroundService`
- Inject `IServiceScopeFactory` (không inject Scoped service trực tiếp vào Singleton)
- Mỗi cycle: tạo scope → đọc interval từ DB → gọi `ScanAndProcessAllDealsAsync` → delay
- Không cần `haveReloadConfig` flag — mỗi cycle đọc config mới từ DB tự động
- Đăng ký `AddHostedService<DealScanBackgroundService>()`

### Bước 11 — Tạo Controllers và DTOs

- Tạo `Models/Dto/`: `DealSearchRequest`, `DealSummaryDto`, `SaveConfigRequest`, `ApiResponse<T>`
- Tạo `Controllers/DealsController.cs`: 3 endpoints (process, list, detail)
- Tạo `Controllers/ConfigController.cs`: GET + PUT config
- Tạo `Controllers/HealthController.cs`: trạng thái background service

### Bước 12 — Viết lại Program.cs

- Thay `[STAThread] static void Main()` bằng ASP.NET Core builder pattern (xem mẫu ở trên)
- `UseWindowsService()` tự detect chạy như Windows Service hay console, không cần code riêng

### Bước 13 — Verify EF Core Migrations

- Migrations cũ giữ nguyên, schema DB không đổi
- Chạy `dotnet ef database update` để verify
- Nếu cần: `dotnet ef migrations add VerifySchema` — nếu migration empty thì xóa đi

### Bước 14 — Thay logging

- Thay tất cả `File.AppendAllText(pathLog, ...)` bằng `ILogger<T>`
- Thêm global exception handler middleware hoặc dùng `.UseExceptionHandler()`
- Cấu hình logging trong `appsettings.json`

### Bước 15 — Dọn dẹp và kiểm thử

**File xóa sau migration:**
- `FormMain.cs`, `FormMain.Designer.cs`, `FormMain.resx`
- `Properties/` folder
- `Models/DatabaseModels.cs`, `Models/BitrixModels.cs`
- `Services/BitrixServices.cs`, `Services/JiraServices.cs`, `Services/BitrixJiraDBServices.cs`
- `Helpers/SendEmail.cs`

**Checklist kiểm thử:**
- [x] Build clean, không có WinForms references — **✅ 0 error, 0 warning**
- [ ] `dotnet run` → Swagger UI tại `/swagger`
- [ ] Background service log xuất hiện sau `IntervalMinutes` phút
- [ ] `POST /api/deals/{id}/process` tạo được issue
- [ ] Gọi `POST /api/deals/{id}/process` 2 lần đồng thời → chỉ 1 issue được tạo
- [ ] Config thay đổi qua `PUT /api/config` → background service nhận interval mới ở cycle tiếp theo

---

## Trạng thái thực hiện (cập nhật 2026-04-21)

### ✅ Đã hoàn thành (Bước 1–14):

| Bước | Trạng thái | Ghi chú |
|---|---|---|
| 1 — Tạo project ASP.NET Core Web API .NET 10 | ✅ | `BitrixJiraConnector.Api/` |
| 2 — Options Pattern | ✅ | `AppSettings.cs`, `appsettings.json`, `ConfigJiraBitrix.cs` (chỉ còn const) |
| 3 — AppDbContext | ✅ | Tách entity ra `Models/Database/`, DI constructor |
| 4 — DealLockService | ✅ | `ConcurrentDictionary<int, SemaphoreSlim>` |
| 5 — DbService | ✅ | Xóa static singleton, toàn bộ async, `GetScanIntervalMinutesAsync` |
| 6 — EmailService | ✅ | Inject `IOptions<EmailSettings>`, xóa SMTP cũ |
| 7 — BitrixService | ✅ | `IHttpClientFactory`, xóa `new HttpClient()`, `throw` thay `throw ex` |
| 8 — JiraService | ✅ | Xóa `using System.Windows.Forms`, `ILogger`, `throw` thay `throw ex` |
| 9 — DealProcessingService | ✅ | Logic điều phối, `DealLockService`, email leo thang |
| 10 — DealScanBackgroundService | ✅ | `IServiceScopeFactory`, đọc interval từ DB mỗi cycle |
| 11 — Controllers + DTOs | ✅ | `DealsController`, `ConfigController`, `HealthController` |
| 12 — Program.cs | ✅ | ASP.NET Core builder, DI đầy đủ, `UseWindowsService` |
| 13 — EF Core Migrations | ✅ | Copy + fix namespace → `BitrixJiraConnector.Api.Migrations` |
| 14 — Logging | ✅ | `ILogger<T>` thay `File.AppendAllText`, `UseExceptionHandler` |

### ⏳ Còn lại:
- Chạy `dotnet ef database update` để apply migrations lên DB
- Test `dotnet run` + Swagger UI
- Kiểm tra end-to-end: scan → tạo issue → email
- Xóa project WinForms cũ sau khi verify xong
