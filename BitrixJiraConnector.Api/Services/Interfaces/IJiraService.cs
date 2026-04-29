using Atlassian.Jira;
using BitrixJiraConnector.Api.Models.Bitrix;

namespace BitrixJiraConnector.Api.Services.Interfaces;

public interface IJiraService
{
    Task<Issue?> CreateIssueAsync(BitrixDataDeal deal);
}
