using AutoMapper;
using MatchService.DTOs;
using MatchService.Interfaces;
using SharedLib.CQRS;

namespace MatchService.Features.GetCurrentMatch
{
    public record GetCurrentMatchQuery(Guid MatchId) : IQuery<MatchInfo>;

    public class GetCurrentMatchHandler : IQueryHandler<GetCurrentMatchQuery, MatchInfo>
    {
        private readonly IMatchRepository _matchRepo;
        private readonly IMapper _mapper;

        public GetCurrentMatchHandler(IMatchRepository matchRepo, IMapper mapper)
        {
            _matchRepo = matchRepo;
            _mapper = mapper;
        }

        public async Task<MatchInfo> Handle(GetCurrentMatchQuery request, CancellationToken cancellationToken)
        {
            var match = await _matchRepo.GetMatchById(request.MatchId);
            if (match == null)
                throw new Exception($"Match with id {request.MatchId} wasn't found");

            return _mapper.Map<MatchInfo>(match);
        }
    }
}
