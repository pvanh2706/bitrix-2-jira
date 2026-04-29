using BitrixJiraConnector.Api.Models.Dto;

namespace BitrixJiraConnector.Api.Services.Interfaces;

public interface IDealProcessingService
{
    Task ScanAndProcessAllDealsAsync(CancellationToken token);
    Task<ProcessDealResult> ProcessSingleDealAsync(int dealId, CancellationToken token);
}
