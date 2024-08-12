using ExpirationService.Models;

namespace ExpirationService.Interfaces;

public interface IExpirationCollectionService
{
    MatchExpirationInfo[] GetExpiredMatches();

    bool Add(MatchExpirationInfo match);

    int Remove(Guid matchId);

    int Count();

    int Count(Guid matchId);
}