using Atlassian.Jira;
using BitrixJiraConnector.Api.Models.Bitrix;
using BitrixJiraConnector.Api.Services.Interfaces;

namespace BitrixJiraConnector.Api.Services;

/// <summary>
/// Chế độ DryRun: không tạo issue thật trên Jira, chỉ log thông tin deal.
/// Bật bằng cách đặt Scanning:DryRun = true trong appsettings.
/// </summary>
public class DryRunJiraService : IJiraService
{
    private readonly ILogger<DryRunJiraService> _logger;

    public DryRunJiraService(ILogger<DryRunJiraService> logger)
    {
        _logger = logger;
    }

    public Task<Issue?> CreateIssueAsync(BitrixDataDeal deal)
    {
        _logger.LogInformation(
            "[DRY RUN] Sẽ tạo issue Jira cho Deal {DealId} | Loại: {LoaiDeal} | Khách sạn: {TenKhachSan} | Pipeline: {Pipeline}",
            deal.DealID,
            deal.LoaiDeal,
            deal.TenKhachSan,
            deal.Pipeline);

        return Task.FromResult<Issue?>(null);
    }
}
