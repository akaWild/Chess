using ExpirationService.Interfaces;
using ExpirationService.Models;
using System.Collections.Concurrent;

namespace ExpirationService.Services;

sealed class ExpirationCollection : IExpirationCollection
{
    private readonly ConcurrentDictionary<MatchExpirationInfo, bool> _matches = new();

    public MatchExpirationInfo[] GetExpiredMatches()
    {
        var expiredMatches = _matches.Where(kv => kv.Key.ExpTimeUtc >= DateTime.UtcNow).Select(kv => kv.Key).ToArray();

        foreach (var match in expiredMatches)
            _matches.TryRemove(match, out _);

        return expiredMatches;
    }

    public void Add(MatchExpirationInfo match)
    {
        _matches.TryAdd(match, true);
    }

    public void Remove(Guid matchId)
    {
        var expiredMatches = _matches.Where(kv => kv.Key.MatchId == matchId).Select(kv => kv.Key);

        foreach (var match in expiredMatches)
            _matches.TryRemove(match, out _);
    }
}