namespace BitrixJiraConnector.Api.Models.Dto;

public class SaveConfigRequest
{
    public int? QuetLaiSau { get; set; }
    public int? GuiLaiEmailSau { get; set; }
    public int? SoNgayQuet { get; set; }
}

public class UpdateSystemConfigRequest
{
    public string Value { get; set; } = "";
}
