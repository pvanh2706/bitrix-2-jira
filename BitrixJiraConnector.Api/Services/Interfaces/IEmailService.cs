namespace BitrixJiraConnector.Api.Services.Interfaces;

public interface IEmailService
{
    /// <param name="ccAddress">Địa chỉ CC thêm (ngoài fallback mặc định). Null = không CC thêm.</param>
    Task SendEmailAsync(string subject, string body, string toAddress, bool includeBitrixJiraAdmins = false, string? ccAddress = null);
}
