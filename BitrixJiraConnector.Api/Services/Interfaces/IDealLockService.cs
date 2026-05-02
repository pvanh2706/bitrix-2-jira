namespace BitrixJiraConnector.Api.Services.Interfaces;

public interface IDealLockService
{
    SemaphoreSlim GetLock(int dealId);
    /// <summary>Trả về true nếu deal đang bị lock (đang được xử lý).</summary>
    bool IsProcessing(int dealId);
}
