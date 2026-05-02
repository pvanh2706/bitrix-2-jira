using BitrixJiraConnector.Api.Models.Dto;
using BitrixJiraConnector.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitrixJiraConnector.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DealsController : ControllerBase
{
    private readonly IDbService _dbService;
    private readonly IDealProcessingService _processingService;
    private readonly IDealLockService _lockService;

    public DealsController(IDbService dbService, IDealProcessingService processingService, IDealLockService lockService)
    {
        _dbService = dbService;
        _processingService = processingService;
        _lockService = lockService;
    }

    [HttpPost("{dealId}/process")]
    public async Task<ActionResult<ApiResponse<ProcessDealResult>>> ProcessDeal(int dealId, CancellationToken token)
    {
        var result = await _processingService.ProcessSingleDealAsync(dealId, token);
        return Ok(ApiResponse<ProcessDealResult>.Ok(result));
    }

    [HttpGet("{dealId}/is-processing")]
    public ActionResult<ApiResponse<bool>> IsDealProcessing(int dealId)
    {
        bool processing = _lockService.IsProcessing(dealId);
        return Ok(ApiResponse<bool>.Ok(processing));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DealSummaryDto>>>> GetDeals(
        [FromQuery] int? dealId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var from = fromDate ?? DateTime.Now.AddDays(-30);
        var to = toDate ?? DateTime.Now;
        var items = await _dbService.SearchDealAsync(dealId, from, to);
        var dtos = items.Select(i => new DealSummaryDto
        {
            Bitrix_DealID = i.Bitrix_DealID,
            Bitrix_DealLink = i.Bitrix_DealLink,
            Bitrix_DateSearch = i.Bitrix_DateSearch,
            IsSendDataToJira = i.IsSendDataToJira,
            Jira_Link = i.Jira_Link,
            HaveError = i.HaveError,
            ErrorInfo = i.ErrorInfo,
            DateTimeCreated = i.DateTimeCreated,
            LastChangeData = i.LastChangeData,
        }).ToList();
        return Ok(ApiResponse<List<DealSummaryDto>>.Ok(dtos));
    }

    [HttpGet("{dealId}")]
    public async Task<ActionResult<ApiResponse<DealSummaryDto>>> GetDeal(int dealId)
    {
        var item = await _dbService.GetDealByDealIdAsync(dealId);
        if (item == null) return NotFound(ApiResponse<DealSummaryDto>.Fail("Deal not found"));
        var dto = new DealSummaryDto
        {
            Bitrix_DealID = item.Bitrix_DealID,
            Bitrix_DealLink = item.Bitrix_DealLink,
            Bitrix_DateSearch = item.Bitrix_DateSearch,
            IsSendDataToJira = item.IsSendDataToJira,
            Jira_Link = item.Jira_Link,
            HaveError = item.HaveError,
            ErrorInfo = item.ErrorInfo,
            DateTimeCreated = item.DateTimeCreated,
            LastChangeData = item.LastChangeData,
        };
        return Ok(ApiResponse<DealSummaryDto>.Ok(dto));
    }
}
