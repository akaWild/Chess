using MatchService.Features.Resign;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Features
{
    public partial class MatchHub
    {
        [Authorize]
        public async Task Resign(Guid matchId)
        {
            var matchFinishedDto = await _sender.Send(new ResignCommand(matchId, Context.User?.Identity?.Name));

            await Clients.Group(matchId.ToString()).SendAsync("MatchFinished", matchFinishedDto);
        }
    }
}
