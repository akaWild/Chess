using AutoMapper;
using ChessDotNet.Public;
using EventsLib;
using MassTransit;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Interfaces;
using MatchService.Models;
using SharedLib.CQRS;

namespace MatchService.Features.AcceptMatch
{
    public record AcceptMatchCommand(Guid MatchId, string? User)
        : ICommand<MatchStartedDto>;

    public class AcceptMatchHandler : ICommandHandler<AcceptMatchCommand, MatchStartedDto>
    {
        private readonly IMatchRepository _matchRepo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public AcceptMatchHandler(IMatchRepository matchRepo, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _matchRepo = matchRepo;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<MatchStartedDto> Handle(AcceptMatchCommand request, CancellationToken cancellationToken)
        {
            if (request.User == null)
                throw new UserNotAuthenticated("User is not authenticated");

            var match = await _matchRepo.GetMatchById(request.MatchId);
            if (match == null)
                throw new MatchNotFoundException($"Match with id {request.MatchId} wasn't found");

            if (match.Status != MatchStatus.Created)
                throw new MatchAcceptanceException("Only not started match can be accepted");

            if (request.User == match.Creator)
                throw new MatchAcceptanceException("Match can't be accepted by match creator");

            match.Acceptor = request.User;
            match.StartedAtUtc = DateTime.UtcNow;

            if (match.WhiteSidePlayer == null)
            {
                var rndResult = Random.Shared.Next(0, 2);

                match.WhiteSidePlayer = rndResult == 0 ? match.Creator : match.Acceptor;
            }
            else
                match.WhiteSidePlayer = match.WhiteSidePlayer == "0" ? match.Creator : match.Acceptor;

            match.ActingSide = MatchSide.White;
            match.Status = MatchStatus.InProgress;
            match.Board = PublicData.DefaultChessPosition;

            if (match.TimeLimit != null)
            {
                match.WhiteSideTimeRemaining = match.TimeLimit;
                match.BlackSideTimeRemaining = match.TimeLimit;
            }

            await _matchRepo.SaveChangesAsync();

            await _publishEndpoint.Publish(_mapper.Map<MatchStarted>(match), cancellationToken);

            return _mapper.Map<MatchStartedDto>(match);
        }
    }
}
