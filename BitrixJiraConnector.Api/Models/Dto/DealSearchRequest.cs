namespace BitrixJiraConnector.Api.Models.Dto;

public class DealSearchRequest
{
    public int? DealId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
