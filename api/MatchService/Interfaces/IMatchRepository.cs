using MatchService.Models;

namespace MatchService.Interfaces;

public interface IMatchRepository
{
    Task<Match?> GetMatchById(Guid matchId);
    void AddMatch(Match match);
    void RemoveMatch(Match match);
    Task<int> SaveChangesAsync();
}