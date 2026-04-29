using BitrixJiraConnector.Api.Models.Bitrix;
using Newtonsoft.Json.Linq;

namespace BitrixJiraConnector.Api.Services.Interfaces;

public interface IBitrixService
{
    Task<JObject> GetCustomFieldsAsync();
    Task<JObject> GetProductListAsync();
    Task<JObject> GetProductByIdAsync(string productId);
    Task<JObject> GetProductSectionListAsync();
    Task<BitrixDataDealApiResult> GetDealByIdAsync(int dealId, JObject customFields);
    Task<UserBitrix> GetResponsibleUserByIdAsync(string userId);
    Task<ContactBitrix> GetContactLienHeKhiTrienKhaiAsync(string dealId);
    Task PostJiraDataToDealAsync(BitrixDataDeal deal, string jiraKey);
    Task<List<int>> GetDealIdsToProcessAsync();
}
