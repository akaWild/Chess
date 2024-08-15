using FluentValidation;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Interfaces;
using MatchService.Models;
using SharedLib.CQRS;

namespace MatchService.Features.CreateMatch
{
    public record CreateMatchCommand(CreateMatchDto CreateMatchDto, string? User)
        : ICommand<MatchCreatedDto>;

    public class CreateMatchValidator
        : AbstractValidator<CreateMatchCommand>
    {
        public CreateMatchValidator()
        {
            RuleFor(x => x.CreateMatchDto).NotNull().WithMessage("Match data object can't be null");
            RuleFor(x => x.CreateMatchDto.MatchId).NotEmpty().WithMessage("Match id must be provided");
            RuleFor(p => p.CreateMatchDto.AILevel).InclusiveBetween(1, 25).WithMessage("AI level must be in the range [1,25]");
            RuleFor(p => new { p.CreateMatchDto.VsBot, p.CreateMatchDto.AILevel }).Must(x => ValidAISettings(x.VsBot, x.AILevel)).WithMessage("AI settings are inconsistent");
            RuleFor(p => p.CreateMatchDto.TimeLimit).InclusiveBetween(180, 7200).WithMessage("Time limit must be in the range [180,7200] seconds");
            RuleFor(p => p.CreateMatchDto.ExtraTimePerMove).InclusiveBetween(5, 300).WithMessage("Extra time per move must be in the range [5,300] seconds");
            RuleFor(p => new { p.CreateMatchDto.TimeLimit, p.CreateMatchDto.ExtraTimePerMove }).Must(x => ValidTimeSettings(x.TimeLimit, x.ExtraTimePerMove)).WithMessage("Extra time per move value can't be provided together with null time limit");
            RuleFor(p => p.CreateMatchDto.FirstToActSide).InclusiveBetween(0, 1).WithMessage("First side to act value must be 0(white) or 1 (black)");

            bool ValidTimeSettings(int? timeLimit, int? extraTime)
            {
                if (timeLimit == null && extraTime != null)
                    return false;

                return true;
            }

            bool ValidAISettings(bool vsBot, int? aiLevel)
            {
                if (vsBot)
                {
                    if (aiLevel == null)
                        return false;
                }
                else
                {
                    if (aiLevel != null)
                        return false;
                }

                return true;
            }
        }
    }

    public class CreateMatchHandler : ICommandHandler<CreateMatchCommand, MatchCreatedDto>
    {
        private readonly IMatchRepository _matchRepo;

        public CreateMatchHandler(IMatchRepository matchRepo)
        {
            _matchRepo = matchRepo;
        }

        public async Task<MatchCreatedDto> Handle(CreateMatchCommand request, CancellationToken cancellationToken)
        {
            if (request.User == null)
                throw new UserNotAuthenticated("User is not authenticated");

            var match = await _matchRepo.GetMatchById(request.CreateMatchDto.MatchId);
            if (match != null)
                throw new DuplicateMatchException($"Match with id {request.CreateMatchDto.MatchId} already exists");

            match = new Match
            {
                MatchId = request.CreateMatchDto.MatchId,
                AILevel = request.CreateMatchDto.AILevel,
                TimeLimit = request.CreateMatchDto.TimeLimit,
                ExtraTimePerMove = request.CreateMatchDto.ExtraTimePerMove,
                WhiteSidePlayer = request.CreateMatchDto.FirstToActSide?.ToString(),
                Creator = request.User
            };

            _matchRepo.AddMatch(match);
            await _matchRepo.SaveChangesAsync();

            var matchCreatedDto = new MatchCreatedDto
            {
                MatchId = match.MatchId,
                CreatedAtUtc = match.CreatedAtUtc,
                Creator = match.Creator
            };

            return matchCreatedDto;
        }
    }
}
