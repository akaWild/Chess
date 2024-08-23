namespace MatchService.DTOs
{
    public record MatchFinishedDto
    {
        public required Guid MatchId { get; init; }
        public required DateTime EndedAtUtc { get; init; }
        public string? Winner { get; init; }
        public string? WinBy { get; init; }
        public string? DrawBy { get; init; }
    }
}
