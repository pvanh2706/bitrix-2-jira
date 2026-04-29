namespace BitrixJiraConnector.Api.Configurations;

public class BitrixSettings
{
    public string ApiUrl { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public string DealDetailUrl { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string AttachFilePath { get; set; } = "";
    public string LogPath { get; set; } = "";
}

public class JiraSettings
{
    public string Url { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
}

public class EmailSettings
{
    public string SendGridApiKey { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "";
    public string FallbackToAddress { get; set; } = "";
    public string RenewalManagerEmail { get; set; } = "";
    public string SalesManagerEmail { get; set; } = "";
    public string AdminBitrixEmail { get; set; } = "";
    public string AdminJiraEmail { get; set; } = "";
}

public class ScanningSettings
{
    public int IntervalMinutes { get; set; } = 5;
    public int LookbackDays { get; set; } = 1;
    public int ResendEmailAfterHours { get; set; } = 50;
}
