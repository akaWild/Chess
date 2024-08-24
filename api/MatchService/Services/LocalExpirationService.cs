using MatchService.Interfaces;
using MatchService.Models;

namespace MatchService.Services
{
    public class LocalExpirationService : ILocalExpirationService
    {
        public string? GetWinner(Match match)
        {
            string? winner = null;

            if (match.WhiteSideTimeRemaining != null)
            {
                if ((DateTime.UtcNow - (match.LastMoveMadeAtUtc ?? match.StartedAtUtc!)).Value.TotalSeconds >= match.WhiteSideTimeRemaining)
                    winner = match.WhiteSidePlayer == match.Creator ? match.Acceptor : match.Creator;
            }

            if (winner == null && match.BlackSideTimeRemaining != null)
            {
                if ((DateTime.UtcNow - (match.LastMoveMadeAtUtc ?? match.StartedAtUtc!)).Value.TotalSeconds >= match.BlackSideTimeRemaining)
                    winner = match.WhiteSidePlayer == match.Creator ? match.Creator : match.Acceptor;
            }

            return winner;
        }
    }
}
