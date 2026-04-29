namespace BitrixJiraConnector.Api.Services.Interfaces;

public interface IDealLockService
{
    SemaphoreSlim GetLock(int dealId);
}
