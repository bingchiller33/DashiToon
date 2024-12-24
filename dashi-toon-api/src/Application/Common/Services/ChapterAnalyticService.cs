using System.Collections.Concurrent;
using DashiToon.Api.Application.ReadContent.Commands.SyncViewCount;
using Microsoft.Extensions.Caching.Memory;

namespace DashiToon.Api.Application.Common.Services;

public class ChapterAnalyticService
{
    private readonly ISender _sender;
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<int, int> _pendingUpdates;
    private readonly Timer _timer;
    private static readonly object Sync = new();

    public ChapterAnalyticService(ISender sender, IMemoryCache cache)
    {
        _sender = sender;
        _cache = cache;
        _timer = new Timer(OnTimerElapsed, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

        _pendingUpdates = new ConcurrentDictionary<int, int>();
    }

    public void IncrementViewCount(int chapterId, string? userId, string? userIp)
    {
        string? key = BuildCacheKey(chapterId, userId, userIp);

        if (_cache.TryGetValue(key, out _))
        {
            return;
        }

        _pendingUpdates.AddOrUpdate(chapterId, 1, (_, viewCount) => viewCount + 1);

        _cache.Set(key, true, TimeSpan.FromSeconds(30));
    }

    private static string BuildCacheKey(int chapterId, string? userId, string? userIp)
    {
        return userId is not null
            ? $"{chapterId}_by_user_{userId}"
            : $"{chapterId}_by_ip_{userIp}";
    }

    private void OnTimerElapsed(object? state)
    {
        lock (Sync)
        {
            if (_pendingUpdates.IsEmpty)
            {
                return;
            }

            _sender.Send(new SyncViewCountCommand(_pendingUpdates.ToDictionary()))
                .GetAwaiter()
                .GetResult();

            _pendingUpdates.Clear();
        }
    }
}
