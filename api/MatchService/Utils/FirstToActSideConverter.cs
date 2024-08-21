using AutoMapper;

namespace MatchService.Utils
{
    public class FirstToActSideConverter : IValueConverter<string?, int?>
    {
        public int? Convert(string? sourceMember, ResolutionContext context)
        {
            if (sourceMember == null)
                return null;

            if (int.TryParse(sourceMember, out var wsp))
                return wsp;

            return null;
        }
    }
}
