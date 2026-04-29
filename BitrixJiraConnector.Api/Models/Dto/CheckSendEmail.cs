using BitrixJiraConnector.Api.Configurations;

namespace BitrixJiraConnector.Api.Models.Dto;

public class CheckSendEmailResult
{
    public bool IsSendMail { get; set; }
    public int SaveTimeSendMailTo { get; set; }
}
