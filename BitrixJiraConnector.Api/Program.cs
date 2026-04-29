using BitrixJiraConnector.Api.BackgroundServices;
using BitrixJiraConnector.Api.Configurations;
using BitrixJiraConnector.Api.Models.Database;
using BitrixJiraConnector.Api.Services;
using BitrixJiraConnector.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();

// Options
builder.Services.Configure<BitrixSettings>(builder.Configuration.GetSection("Bitrix"));
builder.Services.Configure<JiraSettings>(builder.Configuration.GetSection("Jira"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<ScanningSettings>(builder.Configuration.GetSection("Scanning"));

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
builder.Services.AddOpenApi();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler("/error");
app.MapControllers();
app.Run();
