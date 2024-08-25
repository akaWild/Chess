using AutoMapper;
using EventsLib;
using MassTransit;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Interfaces;
using MatchService.Models;
using SharedLib.CQRS;

namespace MatchService.Features.RejectDraw
{
    public record RejectDrawCommand(Guid MatchId, string? User)
        : ICommand<IMatchDto>;

    public class RejectDrawHandler : ICommandHandler<RejectDrawCommand, IMatchDto>
    {
        private readonly IMatchRepository _matchRepo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private readonly ILocalExpirationService _expService;

        public RejectDrawHandler(IMatchRepository matchRepo, IPublishEndpoint publishEndpoint, IMapper mapper, ILocalExpirationService expService)
        {
            _matchRepo = matchRepo;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
            _expService = expService;
        }

        public async Task<IMatchDto> Handle(RejectDrawCommand request, CancellationToken cancellationToken)
        {
            if (request.User == null)
                throw new UserNotAuthenticated("User is not authenticated");

            var match = await _matchRepo.GetMatchById(request.MatchId);
            if (match == null)
                throw new MatchNotFoundException($"Match with id {request.MatchId} wasn't found");

            if (request.User != match.Creator && request.User != match.Acceptor)
                throw new DrawRejectException("Draw can be rejected only by match participant");

            if (match.Status != MatchStatus.InProgress)
                throw new DrawRejectException("Draw can be rejected only on active match");

            if (match.DrawRequestedSide == null)
                throw new DrawRejectException("Can't reject draw because there wasn't previous request");

            if (match.ActingSide == MatchSide.White && match.WhiteSidePlayer != request.User)
                throw new DrawRejectException("Draw can be rejected only by active side of the match");

            if (match.ActingSide == MatchSide.Black && match.WhiteSidePlayer == request.User)
                throw new DrawRejectException("Draw can be rejected only by active side of the match");

            string? winner = _expService.GetWinner(match);

            match.DrawRequestedSide = null;

            if (winner != null)
            {
                match.EndedAtUtc = DateTime.UtcNow;
                match.Status = MatchStatus.Finished;
                match.ActingSide = null;
                match.DrawBy = null;
                match.WinBy = WinDescriptor.OnTime;
                match.Winner = winner;
            }

            await _matchRepo.SaveChangesAsync();

            if (winner != null)
            {
                await _publishEndpoint.Publish(_mapper.Map<MatchFinished>(match), cancellationToken);

                return _mapper.Map<MatchFinishedDto>(match);
            }

            await _publishEndpoint.Publish(new DrawRejected(request.MatchId), cancellationToken);

            return new DrawRejectedDto
            {
                MatchId = request.MatchId,
            };
        }
    }
}
