using System.Collections.Concurrent;
using BitrixJiraConnector.Api.Services.Interfaces;

namespace BitrixJiraConnector.Api.Services;

public class DealLockService : IDealLockService
{
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

    /// <summary>
    /// Trả về SemaphoreSlim(1,1) dành riêng cho từng dealId.
    /// Đảm bảo tại một thời điểm chỉ có 1 luồng xử lý một deal nhất định,
    /// tránh tạo Jira issue trùng khi background scan và API thủ công chạy đồng thời.
    /// </summary>
    public SemaphoreSlim GetLock(int dealId)
        => _locks.GetOrAdd(dealId, _ => new SemaphoreSlim(1, 1));

    public bool IsProcessing(int dealId)
        => _locks.TryGetValue(dealId, out var sem) && sem.CurrentCount == 0;
}
