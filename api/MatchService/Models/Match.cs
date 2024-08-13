namespace MatchService.Models
{
    public class Match
    {
        public required Guid MatchId { get; init; }
        public required string Creator { get; init; }
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? EndedAtUtc { get; set; }
        public DateTime? LastMoveMadeAtUtc { get; set; }
        public int? TimeLimit { get; set; }
        public int? ExtraTimePerMove { get; set; }
        public string? WhiteSidePlayer { get; set; }
        public MatchSide? ActingSide { get; set; }
        public MatchStatus Status { get; set; } = MatchStatus.Created;
        public string? Acceptor { get; set; }
        public int? AILevel { get; set; }
        public string? Board { get; set; }
        public string[] History { get; set; } = { };
        public string? Winner { get; set; }
        public WinDescriptor? WinBy { get; set; }
        public DrawDescriptor? DrawBy { get; set; }
        public MatchSide? DrawRequestedSide { get; set; }
        public int? WhiteSideTimeRemaining { get; set; }
        public int? BlackSideTimeRemaining { get; set; }
    }
}
