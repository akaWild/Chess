namespace EventsLib;

public record SideToActChanged(Guid MatchId, int SideToAct, DateTime ExpTimeUtc);