using BitrixJiraConnector.Api.Models.Database;

namespace BitrixJiraConnector.Api.Services.Interfaces;

public interface IDbService
{
    Task<List<BitrixJiraInfo>> GetListDealCreatedByDayAsync(string beginDateSearch);
    Task InsertDataAsync(BitrixJiraInfo info);
    Task<BitrixJiraInfo?> GetDealByDealIdAsync(int dealId);
    Task SetBitrixCreateIssSuccessAsync(int dealId, string urlIssuesCreated);
    Task UpdateDateTimeSendMailAsync(int dealId, int saveTimeSendMailTo);
    Task<List<BitrixJiraInfo>> SearchDealAsync(int? dealId, DateTime fromDate, DateTime toDate);
    Task AddLogExceptionAsync(ExceptionLog log);
    Task SaveConfigAsync(int? quetLaiSau, int? guiLaiEmailSau, int? soNgayQuet);
    Task<List<ConfigData>> GetConfigDatasAsync();
    Task<int> GetScanIntervalMinutesAsync();
}
