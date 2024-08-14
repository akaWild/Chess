using AutoMapper;
using ChessDotNet;

namespace MatchService.Utils
{
    public class HistoryToPgnConverter : IValueConverter<string[], string>
    {
        public string Convert(string[] sourceMember, ResolutionContext context)
        {
            var chess = new Chess();

            foreach (var move in sourceMember)
                chess.Move(move);

            return chess.GetPgn();
        }
    }
}
