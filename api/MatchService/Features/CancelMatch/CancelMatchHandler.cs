using MatchService.Exceptions;
using MatchService.Interfaces;
using MatchService.Models;
using MediatR;
using SharedLib.CQRS;

namespace MatchService.Features.CancelMatch
{
    public record CancelMatchCommand(Guid MatchId, string User)
        : ICommand<Unit>;

    public class CancelMatchHandler : ICommandHandler<CancelMatchCommand, Unit>
    {
        private readonly IMatchRepository _matchRepo;

        public CancelMatchHandler(IMatchRepository matchRepo)
        {
            _matchRepo = matchRepo;
        }

        public async Task<Unit> Handle(CancelMatchCommand request, CancellationToken cancellationToken)
        {
            var match = await _matchRepo.GetMatchById(request.MatchId);
            if (match == null)
                throw new MatchNotFoundException($"Match with id {request.MatchId} wasn't found");

            if (match.Status != MatchStatus.Created)
                throw new MatchCancellationException("Only not started match can be cancelled");

            if (request.User != match.Creator)
                throw new MatchCancellationException("Match can be cancelled only by match creator");

            _matchRepo.RemoveMatch(match);
            await _matchRepo.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
