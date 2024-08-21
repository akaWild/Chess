namespace EventsLib;

public record MatchCreated
{
    public required Guid MatchId { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required bool VsBot { get; init; }
    public int? AILevel { get; init; }
    public int? TimeLimit { get; init; }
    public int? ExtraTimePerMove { get; init; }
    public int? FirstToActSide { get; init; }
    public required string Creator { get; init; }
}