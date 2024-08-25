using AutoMapper;
using EventsLib;
using MassTransit;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Interfaces;
using MatchService.Models;
using SharedLib.CQRS;

namespace MatchService.Features.Resign
{
    public record ResignCommand(Guid MatchId, string? User)
        : ICommand<MatchFinishedDto>;

    public class ResignHandler : ICommandHandler<ResignCommand, MatchFinishedDto>
    {
        private readonly IMatchRepository _matchRepo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public ResignHandler(IMatchRepository matchRepo, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _matchRepo = matchRepo;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<MatchFinishedDto> Handle(ResignCommand request, CancellationToken cancellationToken)
        {
            if (request.User == null)
                throw new UserNotAuthenticated("User is not authenticated");

            var match = await _matchRepo.GetMatchById(request.MatchId);
            if (match == null)
                throw new MatchNotFoundException($"Match with id {request.MatchId} wasn't found");

            if (request.User != match.Creator && request.User != match.Acceptor)
                throw new ResignException("Resignation can be requested only by match participant");

            if (match.Status != MatchStatus.InProgress)
                throw new ResignException("Resignation can be requested only on active match");

            var winner = request.User == match.Creator ? match.Acceptor : match.Creator;

            match.EndedAtUtc = DateTime.UtcNow;
            match.Status = MatchStatus.Finished;
            match.ActingSide = null;
            match.DrawBy = null;
            match.WinBy = WinDescriptor.Resignation;
            match.Winner = winner;

            await _matchRepo.SaveChangesAsync();

            await _publishEndpoint.Publish(_mapper.Map<MatchFinished>(match), cancellationToken);

            return _mapper.Map<MatchFinishedDto>(match);
        }
    }
}
