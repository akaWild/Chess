using ExpirationService.Models;

namespace ExpirationService.Interfaces;

public interface IExpirationCollection
{
    MatchExpirationInfo[] GetExpiredMatches();

    void Add(MatchExpirationInfo match);

    void Remove(Guid matchId);
}