namespace BitrixJiraConnector.Api.Models.Dto;

public class DealSummaryDto
{
    public int Bitrix_DealID { get; set; }
    public string Bitrix_DealLink { get; set; } = "";
    public string Bitrix_DateSearch { get; set; } = "";
    public int IsSendDataToJira { get; set; }
    public string Jira_Link { get; set; } = "";
    public int HaveError { get; set; }
    public string ErrorInfo { get; set; } = "";
    public string DateTimeCreated { get; set; } = "";
    public string LastChangeData { get; set; } = "";
}
