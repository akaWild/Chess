using MatchService.Interfaces;

namespace MatchService.DTOs;

public record DrawRequestedDto : IMatchDto
{
    public required Guid MatchId { get; init; }
    public required int RequestSide { get; init; }
}