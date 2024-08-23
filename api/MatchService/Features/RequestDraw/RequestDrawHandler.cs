using EventsLib;
using MassTransit;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Interfaces;
using MatchService.Models;
using SharedLib.CQRS;

namespace MatchService.Features.RequestDraw
{
    public record RequestDrawCommand(Guid MatchId, string? User)
        : ICommand<DrawRequestedDto>;

    public class RequestDrawHandler : ICommandHandler<RequestDrawCommand, DrawRequestedDto>
    {
        private readonly IMatchRepository _matchRepo;
        private readonly IPublishEndpoint _publishEndpoint;

        public RequestDrawHandler(IMatchRepository matchRepo, IPublishEndpoint publishEndpoint)
        {
            _matchRepo = matchRepo;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<DrawRequestedDto> Handle(RequestDrawCommand request, CancellationToken cancellationToken)
        {
            if (request.User == null)
                throw new UserNotAuthenticated("User is not authenticated");

            var match = await _matchRepo.GetMatchById(request.MatchId);
            if (match == null)
                throw new MatchNotFoundException($"Match with id {request.MatchId} wasn't found");

            if (request.User != match.Creator && request.User != match.Acceptor)
                throw new DrawRequestException("Draw can be requested only by match participant");

            if (match.Status != MatchStatus.InProgress)
                throw new DrawRequestException("Draw can be requested only on active match");

            if (match.DrawRequestedSide != null)
                throw new DrawRequestException("Draw has been already requested");

            if (match.ActingSide == MatchSide.White && match.WhiteSidePlayer == request.User)
                throw new DrawRequestException("Draw can be requested only by idle side of the match");

            if (match.ActingSide == MatchSide.Black && match.WhiteSidePlayer != request.User)
                throw new DrawRequestException("Draw can be requested only by idle side of the match");

            match.DrawRequestedSide = match.ActingSide == MatchSide.White ? MatchSide.Black : MatchSide.White;

            _matchRepo.RemoveMatch(match);
            await _matchRepo.SaveChangesAsync();

            await _publishEndpoint.Publish(new DrawRequested(request.MatchId, (int)match.DrawRequestedSide), cancellationToken);

            return new DrawRequestedDto
            {
                MatchId = request.MatchId,
                RequestSide = (int)match.DrawRequestedSide
            };
        }
    }
}
