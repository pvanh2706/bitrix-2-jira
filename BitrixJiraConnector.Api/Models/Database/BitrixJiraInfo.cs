using System.ComponentModel.DataAnnotations;

namespace BitrixJiraConnector.Api.Models.Database;

public class BitrixJiraInfo
{
    [Key]
    public int Bitrix_DealID { get; set; }
    public string Bitrix_DealLink { get; set; } = "";
    public string Bitrix_DateSearch { get; set; } = "";
    public int IsSendDataToJira { get; set; }
    public int IsSendEmail { get; set; }
    public string Jira_Link { get; set; } = "";
    public int HaveError { get; set; }
    public string ErrorInfo { get; set; } = "";
    public string DateTimeCreated { get; set; } = "";
    public int NumberCheckError { get; set; }
    public long? DateTimeSendMailFirst { get; set; }
    public long? DateTimeSendMailSecond { get; set; }
    public long? DateTimeSendMailThird { get; set; }
    public string LastChangeData { get; set; } = "";
}
