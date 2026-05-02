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

public class ScanningSettings
{
    public bool DryRun { get; set; } = false;
    /// <summary>Giả lập thời gian xử lý khi DryRun = true. Dùng để test concurrency.</summary>
    public int DryRunDelaySeconds { get; set; } = 0;
}
