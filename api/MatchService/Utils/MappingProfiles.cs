using AutoMapper;
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
        }
    }
}
