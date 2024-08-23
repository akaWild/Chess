using MatchService.Features.RequestDraw;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Features
{
    public partial class MatchHub
    {
        [Authorize]
        public async Task RequestDraw(Guid matchId)
        {
            var drawRequestedDto = await _sender.Send(new RequestDrawCommand(matchId, Context.User?.Identity?.Name));

            await Clients.Group(matchId.ToString()).SendAsync("DrawRequested", drawRequestedDto);
        }
    }
}
