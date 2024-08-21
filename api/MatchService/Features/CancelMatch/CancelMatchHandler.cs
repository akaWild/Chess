using EventsLib;
using MassTransit;
using MatchService.Exceptions;
using MatchService.Interfaces;
using MatchService.Models;
using MediatR;
using SharedLib.CQRS;

namespace MatchService.Features.CancelMatch
{
    public record CancelMatchCommand(Guid MatchId, string? User)
        : ICommand<Unit>;

    public class CancelMatchHandler : ICommandHandler<CancelMatchCommand, Unit>
    {
        private readonly IMatchRepository _matchRepo;
        private readonly IPublishEndpoint _publishEndpoint;

        public CancelMatchHandler(IMatchRepository matchRepo, IPublishEndpoint publishEndpoint)
        {
            _matchRepo = matchRepo;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Unit> Handle(CancelMatchCommand request, CancellationToken cancellationToken)
        {
            if (request.User == null)
                throw new UserNotAuthenticated("User is not authenticated");

            var match = await _matchRepo.GetMatchById(request.MatchId);
            if (match == null)
                throw new MatchNotFoundException($"Match with id {request.MatchId} wasn't found");

            if (match.Status != MatchStatus.Created)
                throw new MatchCancellationException("Only not started match can be cancelled");

            if (request.User != match.Creator)
                throw new MatchCancellationException("Match can be cancelled only by match creator");

            _matchRepo.RemoveMatch(match);
            await _matchRepo.SaveChangesAsync();

            await _publishEndpoint.Publish(new MatchCancelled(request.MatchId), cancellationToken);

            return Unit.Value;
        }
    }
}
