using ExpirationService.Models;

namespace ExpirationService.Interfaces;

public interface IExpirationCollectionService
{
    MatchExpirationInfo[] GetExpiredMatches();

    void Add(MatchExpirationInfo match);

    void Remove(Guid matchId);
}