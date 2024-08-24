using MatchService.DTOs;
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
            var dto = await _sender.Send(new RequestDrawCommand(matchId, Context.User?.Identity?.Name));

            if (dto is DrawRequestedDto)
                await Clients.Group(matchId.ToString()).SendAsync("DrawRequested", dto);
            else
                await Clients.Group(matchId.ToString()).SendAsync("MatchFinished", dto);
        }
    }
}
