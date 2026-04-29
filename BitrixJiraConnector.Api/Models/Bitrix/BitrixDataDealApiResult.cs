namespace BitrixJiraConnector.Api.Models.Bitrix;

public class BitrixDataDealApiResult
{
    public BitrixDataDeal? DataDeal { get; set; }
    public bool HaveError { get; set; }
    public bool HaveCreateIssues { get; set; }
    public bool HaveGetLate { get; set; }
    public string Message { get; set; } = "";
    public string ToAddressEmail { get; set; } = "";
}
