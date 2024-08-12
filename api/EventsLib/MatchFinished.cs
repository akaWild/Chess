namespace EventsLib;

public record MatchFinished(
    Guid MatchId,
    DateTime EndedAtUtc,
    string? Winner,
    string? WinBy,
    string? DrawBy);