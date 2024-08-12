namespace ExpirationService.Models;

public readonly record struct MatchExpirationInfo(Guid MatchId, int SideToAct, DateTime ExpTimeUtc);