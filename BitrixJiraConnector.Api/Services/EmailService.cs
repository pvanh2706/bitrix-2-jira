using BitrixJiraConnector.Api.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BitrixJiraConnector.Api.Services;

public class EmailService : IEmailService
{
    private readonly IDbService _dbService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IDbService dbService, ILogger<EmailService> logger)
    {
        _dbService = dbService;
        _logger = logger;
    }

    public async Task SendEmailAsync(string subject, string body, string toAddress, bool includeBitrixJiraAdmins = false, string? ccAddress = null)
    {
        var all = (await _dbService.GetAllSystemConfigsAsync())
                  .GroupBy(c => c.ConfigKey)
                  .ToDictionary(g => g.Key, g => g.First().ConfigValue);
        string apiKey   = all.GetValueOrDefault("email_sendgrid_api_key", "");
        string fromAddr = all.GetValueOrDefault("email_from_address", "");
        string fromName = all.GetValueOrDefault("email_from_name", "");
        string fallback = all.GetValueOrDefault("email_fallback_to", "");

        // Nếu toAddress rỗng (chưa cấu hình email người phụ trách), dùng fallback để không bỏ sót
        string effectiveTo = string.IsNullOrEmpty(toAddress) ? fallback : toAddress;
        try
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromAddr, fromName);
            var to = new EmailAddress(effectiveTo);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

            // Luôn CC fallback (thường là admin) để không bỏ sót bất kỳ email nào
            msg.AddCc(new EmailAddress(fallback));

            // CC thêm địa chỉ cụ thể — dùng khi leo thang lần 3 (CC người phụ trách để họ biết)
            if (!string.IsNullOrEmpty(ccAddress) && ccAddress != effectiveTo && ccAddress != fallback)
                msg.AddCc(new EmailAddress(ccAddress));

            if (includeBitrixJiraAdmins)
            {
                msg.AddCc(new EmailAddress(all.GetValueOrDefault("email_admin_bitrix", "")));
                msg.AddCc(new EmailAddress(all.GetValueOrDefault("email_admin_jira", "")));
            }
            await client.SendEmailAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", effectiveTo, subject);
        }
    }
}
