﻿using ExpirationService.Interfaces;
using ExpirationService.Models;
using System.Collections.Concurrent;

namespace ExpirationService.Services;

public sealed class ExpirationCollectionService : IExpirationCollectionService
{
    private readonly ConcurrentDictionary<MatchExpirationInfo, bool> _matches = new();

    public MatchExpirationInfo[] GetExpiredMatches()
    {
        var expiredMatches = _matches.Where(kv => kv.Key.ExpTimeUtc <= DateTime.UtcNow).Select(kv => kv.Key).ToArray();

        foreach (var match in expiredMatches)
            _matches.TryRemove(match, out _);

        return expiredMatches;
    }

    public bool Add(MatchExpirationInfo match)
    {
        return _matches.TryAdd(match, true);
    }

    public int Remove(Guid matchId)
    {
        var expiredMatches = _matches.Where(kv => kv.Key.MatchId == matchId).Select(kv => kv.Key).ToArray();

        foreach (var match in expiredMatches)
            _matches.TryRemove(match, out _);

        return expiredMatches.Length;
    }

    public int Count()
    {
        return _matches.Count;
    }

    public int Count(Guid matchId)
    {
        return _matches.Count(kv => kv.Key.MatchId == matchId);
    }
}