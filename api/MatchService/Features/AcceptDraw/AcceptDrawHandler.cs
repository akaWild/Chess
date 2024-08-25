using AutoMapper;
using EventsLib;
using MassTransit;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Interfaces;
using MatchService.Models;
using SharedLib.CQRS;

namespace MatchService.Features.AcceptDraw
{
    public record AcceptDrawCommand(Guid MatchId, string? User)
        : ICommand<MatchFinishedDto>;

    public class AcceptDrawHandler : ICommandHandler<AcceptDrawCommand, MatchFinishedDto>
    {
        private readonly IMatchRepository _matchRepo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private readonly ILocalExpirationService _expService;

        public AcceptDrawHandler(IMatchRepository matchRepo, IPublishEndpoint publishEndpoint, IMapper mapper, ILocalExpirationService expService)
        {
            _matchRepo = matchRepo;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
            _expService = expService;
        }

        public async Task<MatchFinishedDto> Handle(AcceptDrawCommand request, CancellationToken cancellationToken)
        {
            if (request.User == null)
                throw new UserNotAuthenticated("User is not authenticated");

            var match = await _matchRepo.GetMatchById(request.MatchId);
            if (match == null)
                throw new MatchNotFoundException($"Match with id {request.MatchId} wasn't found");

            if (request.User != match.Creator && request.User != match.Acceptor)
                throw new DrawAcceptException("Draw can be requested only by match participant");

            if (match.Status != MatchStatus.InProgress)
                throw new DrawAcceptException("Draw can be requested only on active match");

            if (match.DrawRequestedSide == null)
                throw new DrawAcceptException("Can't accept draw because there wasn't previous request");

            if (match.ActingSide == MatchSide.White && match.WhiteSidePlayer != request.User)
                throw new DrawAcceptException("Draw can be accepted only by active side of the match");

            if (match.ActingSide == MatchSide.Black && match.WhiteSidePlayer == request.User)
                throw new DrawAcceptException("Draw can be accepted only by active side of the match");

            string? winner = _expService.GetWinner(match);

            match.EndedAtUtc = DateTime.UtcNow;
            match.Status = MatchStatus.Finished;
            match.ActingSide = null;

            if (winner != null)
            {
                match.DrawRequestedSide = null;
                match.DrawBy = null;
                match.WinBy = WinDescriptor.OnTime;
                match.Winner = winner;
            }
            else
            {
                match.WinBy = null;
                match.Winner = null;
                match.DrawBy = DrawDescriptor.Agreement;
            }

            _matchRepo.RemoveMatch(match);
            await _matchRepo.SaveChangesAsync();

            await _publishEndpoint.Publish(_mapper.Map<MatchFinished>(match), cancellationToken);

            return _mapper.Map<MatchFinishedDto>(match);
        }
    }
}
