namespace EventsLib;

public record MatchStarted(
    Guid MatchId,
    DateTime StartedAtUtc,
    string Acceptor,
    string WhiteSidePlayer);