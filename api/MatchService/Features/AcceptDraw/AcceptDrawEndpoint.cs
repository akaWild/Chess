using MatchService.Features.AcceptDraw;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Features
{
    public partial class MatchHub
    {
        [Authorize]
        public async Task AcceptDraw(Guid matchId)
        {
            var matchFinishedDto = await _sender.Send(new AcceptDrawCommand(matchId, Context.User?.Identity?.Name));

            await Clients.Group(matchId.ToString()).SendAsync("MatchFinished", matchFinishedDto);
        }
    }
}
