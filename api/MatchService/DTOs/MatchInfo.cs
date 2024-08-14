namespace MatchService.DTOs;

public struct MatchInfo
{
    public required Guid MatchId { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? EndedAtUtc { get; init; }
    public DateTime? LastMoveMadeAtUtc { get; init; }
    public int? TimeLimit { get; init; }
    public int? ExtraTimePerMove { get; init; }
    public string? WhiteSidePlayer { get; init; }
    public required string Status { get; init; }
    public required string Creator { get; init; }
    public string? Acceptor { get; init; }
    public int? AILevel { get; init; }
    public string? Fen { get; init; }
    public string? Pgn { get; init; }
    public string? Winner { get; init; }
    public string? WinBy { get; init; }
    public string? DrawBy { get; init; }
    public int? DrawRequestedSide { get; init; }
    public int? WhiteSideTimeRemaining { get; init; }
    public int? BlackSideTimeRemaining { get; init; }
}
