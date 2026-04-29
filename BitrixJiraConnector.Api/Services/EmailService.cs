using BitrixJiraConnector.Api.Configurations;
using BitrixJiraConnector.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BitrixJiraConnector.Api.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string subject, string body, string toAddress, bool includeBitrixJiraAdmins = false)
    {
        string effectiveTo = string.IsNullOrEmpty(toAddress) ? _settings.FallbackToAddress : toAddress;
        try
        {
            var client = new SendGridClient(_settings.SendGridApiKey);
            var from = new EmailAddress(_settings.FromAddress, _settings.FromName);
            var to = new EmailAddress(effectiveTo);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);
            msg.AddCc(new EmailAddress(_settings.FallbackToAddress));
            if (includeBitrixJiraAdmins)
            {
                msg.AddCc(new EmailAddress(_settings.AdminBitrixEmail));
                msg.AddCc(new EmailAddress(_settings.AdminJiraEmail));
            }
            await client.SendEmailAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", effectiveTo, subject);
        }
    }
}
