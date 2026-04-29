namespace BitrixJiraConnector.Api.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string subject, string body, string toAddress, bool includeBitrixJiraAdmins = false);
}
