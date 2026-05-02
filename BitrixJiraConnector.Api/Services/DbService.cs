using BitrixJiraConnector.Api.Configurations;
using BitrixJiraConnector.Api.Models.Database;
using BitrixJiraConnector.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BitrixJiraConnector.Api.Services;

public class DbService : IDbService
{
    private readonly AppDbContext _db;

    public DbService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<BitrixJiraInfo>> GetListDealCreatedByDayAsync(string beginDateSearch)
    {
        return await _db.BitrixJiraInfoes
            .Where(i => string.Compare(i.Bitrix_DateSearch, beginDateSearch) >= 0)
            .ToListAsync();
    }

    public async Task InsertDataAsync(BitrixJiraInfo info)
    {
        bool exists = await _db.BitrixJiraInfoes.AnyAsync(i => i.Bitrix_DealID == info.Bitrix_DealID);
        if (!exists)
        {
            _db.BitrixJiraInfoes.Add(info);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<BitrixJiraInfo?> GetDealByDealIdAsync(int dealId)
    {
        return await _db.BitrixJiraInfoes.FirstOrDefaultAsync(i => i.Bitrix_DealID == dealId);
    }

    public async Task SetBitrixCreateIssSuccessAsync(int dealId, string urlIssuesCreated)
    {
        var item = await _db.BitrixJiraInfoes.FirstOrDefaultAsync(i => i.Bitrix_DealID == dealId);
        if (item != null)
        {
            item.HaveError = 0;
            item.Jira_Link = urlIssuesCreated;
            await _db.SaveChangesAsync();
        }
    }

    public async Task UpdateDateTimeSendMailAsync(int dealId, int saveTimeSendMailTo)
    {
        var item = await _db.BitrixJiraInfoes.FirstOrDefaultAsync(i => i.Bitrix_DealID == dealId);
        if (item != null)
        {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (saveTimeSendMailTo == (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_FIRST) item.DateTimeSendMailFirst = now;
            if (saveTimeSendMailTo == (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_SECOND) item.DateTimeSendMailSecond = now;
            if (saveTimeSendMailTo == (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_THIRD) item.DateTimeSendMailThird = now;
            item.LastChangeData = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<BitrixJiraInfo>> SearchDealAsync(int? dealId, DateTime fromDate, DateTime toDate, int page = 1, int pageSize = 50)
    {
        string fromStr = fromDate.ToString("yyyy/MM/dd");
        string toStr = toDate.ToString("yyyy/MM/dd");

        return await _db.BitrixJiraInfoes
            .Where(i =>
                (dealId == null || i.Bitrix_DealID == dealId)
                && string.Compare(i.Bitrix_DateSearch, fromStr) >= 0
                && string.Compare(i.Bitrix_DateSearch, toStr) <= 0)
            .OrderByDescending(i => i.LastChangeData)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task AddLogExceptionAsync(ExceptionLog log)
    {
        _db.ExceptionLog.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task SaveConfigAsync(int? quetLaiSau, int? guiLaiEmailSau, int? soNgayQuet)
    {
        await UpdateConfigKey("QuetLaiSau", quetLaiSau?.ToString());
        await UpdateConfigKey("GuiLaiEmailSau", guiLaiEmailSau?.ToString());
        await UpdateConfigKey("SoNgayQuet", soNgayQuet?.ToString());
        await _db.SaveChangesAsync();
    }

    private async Task UpdateConfigKey(string key, string? value)
    {
        if (value == null) return;
        var item = await _db.ConfigData.FirstOrDefaultAsync(i => i.KeyConfig == key);
        if (item != null) item.ValueConfig = value;
    }

    public async Task<List<ConfigData>> GetConfigDatasAsync()
    {
        return await _db.ConfigData.ToListAsync();
    }

    public async Task<int> GetScanIntervalMinutesAsync()
    {
        var item = await _db.ConfigData.FirstOrDefaultAsync(i => i.KeyConfig == "QuetLaiSau");
        if (item != null && int.TryParse(item.ValueConfig, out int val)) return val;
        return 5;
    }

    public async Task<List<string>> GetRequiredFieldsAsync(string dealTypeId)
    {
        return await _db.DealTypeRequiredFields
            .Where(f => f.DealTypeId == dealTypeId)
            .Select(f => f.FieldKey)
            .ToListAsync();
    }

    public async Task<Dictionary<string, string>> GetAllFieldLabelsAsync()
    {
        return await _db.BitrixFieldMappings
            .ToDictionaryAsync(f => f.FieldKey, f => f.FieldLabel);
    }

    public async Task<string?> GetJiraUsernameForEmailAsync(string email)
    {
        var item = await _db.UserEmailMappings.FirstOrDefaultAsync(u => u.Email == email);
        return item?.JiraUsername;
    }

    public async Task<string?> GetPipelineNameAsync(string categoryId)
    {
        var item = await _db.PipelineMappings.FirstOrDefaultAsync(p => p.CategoryId == categoryId);
        return item?.PipelineName;
    }

    public async Task<string?> GetSystemConfigAsync(string key)
    {
        var item = await _db.SystemConfigs.FirstOrDefaultAsync(c => c.ConfigKey == key);
        return item?.ConfigValue;
    }

    public async Task<List<SystemConfig>> GetAllSystemConfigsAsync()
    {
        // GroupBy ConfigKey để loại bỏ duplicate nếu migration seed trùng
        var all = await _db.SystemConfigs.OrderBy(c => c.ConfigKey).ToListAsync();
        return all.GroupBy(c => c.ConfigKey).Select(g => g.First()).ToList();
    }

    public async Task UpdateSystemConfigAsync(string key, string value)
    {
        var item = await _db.SystemConfigs.FirstOrDefaultAsync(c => c.ConfigKey == key);
        if (item == null) return;
        item.ConfigValue = value;
        await _db.SaveChangesAsync();
    }
}
