namespace EventsLib;

public record MatchCreated(
    Guid MatchId,
    DateTime CreatedAtUtc,
    bool VsBot,
    int? AILevel,
    int? TimeLimit,
    int? ExtraTimePerMove,
    int? FirstToActSide,
    string Creator);