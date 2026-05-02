using BitrixJiraConnector.Api.Configurations;
using BitrixJiraConnector.Api.Models.Bitrix;
using BitrixJiraConnector.Api.Models.Database;
using BitrixJiraConnector.Api.Models.Dto;
using BitrixJiraConnector.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using CheckSendEmailResult = BitrixJiraConnector.Api.Models.Dto.CheckSendEmailResult;

namespace BitrixJiraConnector.Api.Services;

public class DealProcessingService : IDealProcessingService
{
    private readonly IBitrixService _bitrixService;
    private readonly IJiraService _jiraService;
    private readonly IDbService _dbService;
    private readonly IEmailService _emailService;
    private readonly IDealLockService _lockService;
    private readonly ILogger<DealProcessingService> _logger;
    private readonly ScanningSettings _scanningSettings;

    public DealProcessingService(
        IBitrixService bitrixService,
        IJiraService jiraService,
        IDbService dbService,
        IEmailService emailService,
        IDealLockService lockService,
        ILogger<DealProcessingService> logger,
        IOptions<ScanningSettings> scanningSettings)
    {
        _bitrixService = bitrixService;
        _jiraService = jiraService;
        _dbService = dbService;
        _emailService = emailService;
        _lockService = lockService;
        _logger = logger;
        _scanningSettings = scanningSettings.Value;
    }

    public async Task ScanAndProcessAllDealsAsync(CancellationToken token)
    {
        _logger.LogInformation("Starting deal scan");
        try
        {
            var dealIds = await _bitrixService.GetDealIdsToProcessAsync();

            // if (_scanningSettings.DryRun)
            // {
            //     _logger.LogInformation("[DRY RUN] Tìm thấy {Count} deal: [{Ids}]",
            //         dealIds.Count,
            //         string.Join(", ", dealIds));
            //     return;
            // }

            foreach (int dealId in dealIds)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    await ProcessSingleDealAsync(dealId, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing deal {DealId}", dealId);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Deal scan cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during deal scan");
        }
    }

    public async Task<ProcessDealResult> ProcessSingleDealAsync(int dealId, CancellationToken token)
    {
        var sem = _lockService.GetLock(dealId); // Lấy Semaphore riêng cho dealId
        bool acquired = await sem.WaitAsync(TimeSpan.FromSeconds(30), token); // Timeout để tránh deadlock nếu có vấn đề với Semaphore
        if (!acquired)
            return new ProcessDealResult { Success = false, Message = "Timeout waiting for deal lock" };

        try
        {
            return await ProcessDealInternalAsync(dealId); // Thực hiện xử lý deal trong phương thức riêng để đảm bảo Semaphore được giải phóng đúng cách
        }
        finally
        {
            sem.Release(); // Đảm bảo giải phóng Semaphore dù có lỗi hay không, tránh tình trạng deadlock
        }
    }

    private async Task<ProcessDealResult> ProcessDealInternalAsync(int dealId)
    {
        string beginDateSearch = DateTime.Now.ToString("yyyy/MM/dd");

        var customFields = await _bitrixService.GetCustomFieldsAsync();
        var dealResult = await _bitrixService.GetDealByIdAsync(dealId, customFields);

        if (_scanningSettings.DryRun)
        {
            _logger.LogInformation(
                "[DRY RUN] Deal {DealId} | LoaiDeal: {LoaiDeal} | TenKhachSan: {TenKhachSan} | HaveError: {HaveError} | HaveCreateIssues: {HaveCreateIssues} | HaveGetLate: {HaveGetLate} | Message: {Message}",
                dealId,
                dealResult.DataDeal?.LoaiDeal ?? "(chưa parse)",
                dealResult.DataDeal?.TenKhachSan ?? "(chưa parse)",
                dealResult.HaveError,
                dealResult.HaveCreateIssues,
                dealResult.HaveGetLate,
                dealResult.Message);
            if (_scanningSettings.DryRunDelaySeconds > 0)
            {
                _logger.LogInformation("[DRY RUN] Giả lập xử lý {Seconds}s cho deal {DealId}...", _scanningSettings.DryRunDelaySeconds, dealId);
                await Task.Delay(TimeSpan.FromSeconds(_scanningSettings.DryRunDelaySeconds));
            }
            return new ProcessDealResult { Success = true, Message = $"[DRY RUN] Deal {dealId} logged, dừng xử lý." };
        }

        if (dealResult.HaveGetLate)
            return new ProcessDealResult { Success = true, Message = "Deal vừa được sửa đổi — tạm thời bỏ qua" };

        if (dealResult.HaveError)
        {
            await HandleDealErrorAsync(dealId, dealResult, beginDateSearch);
            return new ProcessDealResult { Success = false, Message = dealResult.Message };
        }

        // Nếu đã tạo issue thì lưu lại thông tin và dừng xử lý, không tạo lại issue
        if (dealResult.HaveCreateIssues)
        {
            var existing = await _dbService.GetDealByDealIdAsync(dealId);
            if (existing == null)
            {
                await _dbService.InsertDataAsync(new BitrixJiraInfo
                {
                    Bitrix_DealID = dealId,
                    Bitrix_DealLink = dealResult.DataDeal?.LinkCRM ?? "",
                    Bitrix_DateSearch = beginDateSearch,
                    IsSendDataToJira = 0,
                    IsSendEmail = 0,
                    Jira_Link = "",
                    HaveError = 1,
                    ErrorInfo = dealResult.Message,
                    DateTimeCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    NumberCheckError = 0,
                    LastChangeData = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                });
            }
            return new ProcessDealResult { Success = true, Message = dealResult.Message };
        }

        // Create Jira issue
        var deal = dealResult.DataDeal!;
        var issue = await _jiraService.CreateIssueAsync(deal);
        if (issue?.Key == null)
            return new ProcessDealResult { Success = false, Message = "Issue creation returned null key" };

        string jiraKey = issue.Key.ToString();
        string jiraUrl = await BuildJiraUrlAsync(jiraKey);

        // Lưu DB TRƯỚC khi gọi PostJiraDataToDealAsync để tránh tạo duplicate issue khi retry.
        // Nếu PostJiraDataToDealAsync thất bại ở bước sau, DB đã ghi jiraKey →
        // lần scan tiếp theo sẽ thấy HaveCreateIssues = true và dừng lại.
        var dbItem = await _dbService.GetDealByDealIdAsync(dealId);
        if (dbItem != null)
        {
            await _dbService.SetBitrixCreateIssSuccessAsync(dealId, jiraUrl);
        }
        else
        {
            await _dbService.InsertDataAsync(new BitrixJiraInfo
            {
                Bitrix_DealID = dealId,
                Bitrix_DealLink = deal.LinkCRM,
                Bitrix_DateSearch = beginDateSearch,
                IsSendDataToJira = 1,
                IsSendEmail = 1,
                Jira_Link = jiraUrl,
                HaveError = 0,
                ErrorInfo = "",
                DateTimeCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                NumberCheckError = 0,
                LastChangeData = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            });
        }

        // Ghi link Jira về Bitrix — nếu fail thì log lỗi nhưng không re-throw.
        // DB đã lưu jiraKey nên retry sẽ không tạo thêm issue.
        try
        {
            await _bitrixService.PostJiraDataToDealAsync(deal, jiraKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "PostJiraDataToDealAsync thất bại cho deal {DealId} / {JiraKey} — link Jira CHƯA được ghi vào Bitrix, cần xử lý thủ công",
                dealId, jiraKey);
        }

        string successBody = BuildSuccessEmailHtml(deal.LinkCRM, jiraUrl);
        await _emailService.SendEmailAsync(
            ConfigJiraBitrix.MailInfo_Subject_Create_Iss_Success + " - DealID: " + dealId,
            successBody,
            deal.Responsible_Email);

        _logger.LogInformation("Successfully created Jira issue {JiraKey} for deal {DealId}", jiraKey, dealId);
        return new ProcessDealResult { Success = true, JiraKey = jiraKey, JiraUrl = jiraUrl, Message = "Issue created" };
    }

    private async Task HandleDealErrorAsync(int dealId, BitrixDataDealApiResult dealResult, string beginDateSearch)
    {
        string subject = ConfigJiraBitrix.MailInfo_Subject_Create_Iss_Error + " - DealID: " + dealId;
        var dbItem = await _dbService.GetDealByDealIdAsync(dealId);

        if (dbItem == null)
        {
            await _emailService.SendEmailAsync(subject, dealResult.Message, dealResult.ToAddressEmail);
            await _dbService.InsertDataAsync(new BitrixJiraInfo
            {
                Bitrix_DealID = dealId,
                Bitrix_DealLink = dealResult.DataDeal?.LinkCRM ?? "",
                Bitrix_DateSearch = beginDateSearch,
                IsSendDataToJira = 0,
                IsSendEmail = 1,
                Jira_Link = "",
                HaveError = 1,
                ErrorInfo = dealResult.Message,
                DateTimeCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                NumberCheckError = 0,
                DateTimeSendMailFirst = DateTimeOffset.Now.ToUnixTimeSeconds(),
                LastChangeData = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            });
        }
        else
        {
            var checkResult = CheckResendEmail(dbItem);
            if (!checkResult.IsSendMail) return;

            string pipeline = dealResult.DataDeal?.Pipeline ?? "";
            bool isRenewal = pipeline == ((int)TYPE_PIPE_LINE.RENEWAL).ToString();
            string managerEmail = isRenewal
                ? (await GetEmailSettingAsync("email_renewal_manager"))
                : (await GetEmailSettingAsync("email_sales_manager"));

            bool isThird = checkResult.SaveTimeSendMailTo == (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_THIRD;
            string toEmail = isThird ? managerEmail : dealResult.ToAddressEmail;

            await _emailService.SendEmailAsync(subject, dealResult.Message, toEmail);
            await _dbService.UpdateDateTimeSendMailAsync(dealId, checkResult.SaveTimeSendMailTo);
        }
    }

    private static CheckSendEmailResult CheckResendEmail(BitrixJiraInfo info)
    {
        var result = new CheckSendEmailResult { IsSendMail = false, SaveTimeSendMailTo = (int)SAVE_TIME_SEND_MAIL_TO.NO_SAVE };

        if (info.DateTimeSendMailFirst == null)
        {
            result.IsSendMail = true;
            result.SaveTimeSendMailTo = (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_FIRST;
            return result;
        }
        if (info.DateTimeSendMailThird != null) return result;

        DateTime firstSent = DateTimeOffset.FromUnixTimeSeconds(info.DateTimeSendMailFirst ?? 0).LocalDateTime;
        DateTime at17h = new(firstSent.Year, firstSent.Month, firstSent.Day, 17, 0, 0);
        DateTime now = DateTime.Now;
        DateTime nextDay9am = firstSent.AddDays(1).Date.AddHours(9);

        bool shouldSendSecond = (firstSent < at17h && now > at17h) || (firstSent >= at17h && now > nextDay9am);
        if (info.DateTimeSendMailSecond == null && shouldSendSecond)
        {
            result.IsSendMail = true;
            result.SaveTimeSendMailTo = (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_SECOND;
            return result;
        }

        if (info.DateTimeSendMailSecond != null && info.DateTimeSendMailThird == null)
        {
            DateTime secondSent = DateTimeOffset.FromUnixTimeSeconds(info.DateTimeSendMailSecond ?? 0).LocalDateTime;
            DateTime secondNextDay9am = secondSent.AddDays(1).Date.AddHours(9);
            if (now > secondNextDay9am)
            {
                result.IsSendMail = true;
                result.SaveTimeSendMailTo = (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_THIRD;
            }
        }
        return result;
    }

    private async Task<string> GetEmailSettingAsync(string key)
    {
        return await _dbService.GetSystemConfigAsync(key) ?? "";
    }

    private async Task<string> BuildJiraUrlAsync(string jiraKey)
    {
        string baseUrl = await _dbService.GetSystemConfigAsync("jira_url") ?? "https://jira.ezcloudhotel.com";
        return $"{baseUrl}/browse/{jiraKey}";
    }

    private static string BuildSuccessEmailHtml(string dealUrl, string issueUrl) =>
        $"<html><body><p>Issue đã được tạo thành công từ Bitrix</p>" +
        $"<p>Link Deal: <a href='{dealUrl}'>{dealUrl}</a></p>" +
        $"<p>Link Issue: <a href='{issueUrl}'>{issueUrl}</a></p></body></html>";
}
