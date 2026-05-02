using BitrixJiraConnector.Api.Models.Database;

namespace BitrixJiraConnector.Api.Services.Interfaces;

public interface IDbService
{
    Task<List<BitrixJiraInfo>> GetListDealCreatedByDayAsync(string beginDateSearch);
    Task InsertDataAsync(BitrixJiraInfo info);
    Task<BitrixJiraInfo?> GetDealByDealIdAsync(int dealId);
    Task SetBitrixCreateIssSuccessAsync(int dealId, string urlIssuesCreated);
    Task UpdateDateTimeSendMailAsync(int dealId, int saveTimeSendMailTo);
    Task<List<BitrixJiraInfo>> SearchDealAsync(int? dealId, DateTime fromDate, DateTime toDate, int page = 1, int pageSize = 50);
    Task AddLogExceptionAsync(ExceptionLog log);
    Task SaveConfigAsync(int? quetLaiSau, int? guiLaiEmailSau, int? soNgayQuet);
    Task<List<ConfigData>> GetConfigDatasAsync();
    Task<int> GetScanIntervalMinutesAsync();

    // Config tables
    Task<List<string>> GetRequiredFieldsAsync(string dealTypeId);
    Task<Dictionary<string, string>> GetAllFieldLabelsAsync();
    Task<string?> GetJiraUsernameForEmailAsync(string email);
    Task<string?> GetPipelineNameAsync(string categoryId);
    Task<string?> GetSystemConfigAsync(string key);
    Task<List<SystemConfig>> GetAllSystemConfigsAsync();
    Task UpdateSystemConfigAsync(string key, string value);
}
