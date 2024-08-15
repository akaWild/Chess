namespace MatchService.DTOs;

public record MatchStartedDto
{
    public required Guid MatchId { get; init; }
    public required DateTime StartedAtUtc { get; init; }
    public required string Acceptor { get; init; }
    public required string WhiteSidePlayer { get; init; }
}