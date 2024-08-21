using AutoMapper;
using EventsLib;
using MatchService.DTOs;
using MatchService.Models;

namespace MatchService.Utils
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Match, MatchInfo>()
                .ForMember(d => d.Fen, o => o.MapFrom(s => s.Board))
                .ForMember(d => d.Pgn, o => o.ConvertUsing(new HistoryToPgnConverter(), s => s.History));

            CreateMap<Match, MatchStartedDto>();

            CreateMap<Match, MatchCreated>()
                .ForMember(d => d.VsBot, o => o.MapFrom(s => s.AILevel != null))
                .ForMember(d => d.FirstToActSide, o => o.ConvertUsing(new FirstToActSideConverter(), s => s.WhiteSidePlayer));

            CreateMap<Match, MatchStarted>();
        }
    }
}
