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

    public async Task<List<BitrixJiraInfo>> SearchDealAsync(int? dealId, DateTime fromDate, DateTime toDate)
    {
        string fromStr = fromDate.ToString("yyyy/MM/dd");
        string toStr = toDate.ToString("yyyy/MM/dd");

        return await _db.BitrixJiraInfoes
            .Where(i =>
                (dealId == null || i.Bitrix_DealID == dealId)
                && string.Compare(i.Bitrix_DateSearch, fromStr) >= 0
                && string.Compare(i.Bitrix_DateSearch, toStr) <= 0)
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
}
