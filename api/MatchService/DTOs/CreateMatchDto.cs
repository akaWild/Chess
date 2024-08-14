namespace MatchService.DTOs;

public record CreateMatchDto
{
    public Guid MatchId { get; init; }
    public bool VsBot { get; init; }
    public int? AILevel { get; init; }
    public int? TimeLimit { get; init; }
    public int? ExtraTimePerMove { get; init; }
    public int? FirstToActSide { get; init; }
};