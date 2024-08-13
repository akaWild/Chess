using MatchService.Interfaces;
using MatchService.Models;
using Microsoft.EntityFrameworkCore;

namespace MatchService.Data;

class MatchRepository : IMatchRepository
{
    private readonly DataContext _dbContext;

    public MatchRepository(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Match?> GetMatchById(Guid matchId)
    {
        return await _dbContext.Matches.FirstOrDefaultAsync(m => m.MatchId == matchId);
    }

    public void AddMatch(Match match)
    {
        _dbContext.Matches.Add(match);
    }

    public void RemoveMatch(Match match)
    {
        _dbContext.Remove(match);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}