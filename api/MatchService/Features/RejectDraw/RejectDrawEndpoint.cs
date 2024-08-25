using MatchService.DTOs;
using MatchService.Features.RejectDraw;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Features
{
    public partial class MatchHub
    {
        [Authorize]
        public async Task RejectDraw(Guid matchId)
        {
            var dto = await _sender.Send(new RejectDrawCommand(matchId, Context.User?.Identity?.Name));

            if (dto is DrawRejectedDto)
                await Clients.Group(matchId.ToString()).SendAsync("DrawRejected", dto);
            else
                await Clients.Group(matchId.ToString()).SendAsync("MatchFinished", dto);
        }
    }
}
