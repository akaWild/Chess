using MatchService.Interfaces;

namespace MatchService.DTOs;

public record DrawRejectedDto : IMatchDto
{
    public required Guid MatchId { get; init; }
}