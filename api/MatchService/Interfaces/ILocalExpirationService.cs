using MatchService.Models;

namespace MatchService.Interfaces;

public interface ILocalExpirationService
{
    string? GetWinner(Match match);
}