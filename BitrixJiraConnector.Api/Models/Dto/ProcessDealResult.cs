namespace BitrixJiraConnector.Api.Models.Dto;

public class ProcessDealResult
{
    public bool Success { get; set; }
    public string? JiraKey { get; set; }
    public string? JiraUrl { get; set; }
    public string Message { get; set; } = "";
}
