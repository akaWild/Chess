namespace MatchService.DTOs;

public record MatchCreatedDto
{
    public required Guid MatchId { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required string Creator { get; init; }
};