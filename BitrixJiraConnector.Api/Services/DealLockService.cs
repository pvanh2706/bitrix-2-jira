using System.Collections.Concurrent;
using BitrixJiraConnector.Api.Services.Interfaces;

namespace BitrixJiraConnector.Api.Services;

public class DealLockService : IDealLockService
{
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

    public SemaphoreSlim GetLock(int dealId)
        => _locks.GetOrAdd(dealId, _ => new SemaphoreSlim(1, 1));
}
