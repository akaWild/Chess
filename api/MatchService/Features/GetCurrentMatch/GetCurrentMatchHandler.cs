using MatchService.DTOs;
using MatchService.Interfaces;
using SharedLib.CQRS;

namespace MatchService.Features.GetCurrentMatch
{
    public record GetCurrentMatchQuery(Guid MatchId) : IQuery<MatchInfo>;

    public class GetCurrentMatchHandler : IQueryHandler<GetCurrentMatchQuery, MatchInfo>
    {
        private readonly IMatchRepository _matchRepo;

        public GetCurrentMatchHandler(IMatchRepository matchRepo)
        {
            _matchRepo = matchRepo;
        }

        public async Task<MatchInfo> Handle(GetCurrentMatchQuery request, CancellationToken cancellationToken)
        {
            var match = await _matchRepo.GetMatchById(request.MatchId);
            if (match == null)
                throw new Exception($"Match with id {request.MatchId} wasn't found");

            return new MatchInfo
            {
                MatchId = match.MatchId,
                CreatedAtUtc = match.CreatedAtUtc,
                Creator = match.Creator,
                Status = match.Status.ToString()
            };
        }
    }
}
