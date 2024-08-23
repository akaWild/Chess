namespace MatchService.DTOs;

public record DrawRequestedDto
{
    public required Guid MatchId { get; init; }
    public required int RequestSide { get; init; }
}