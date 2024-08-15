using MatchService.Features.CancelMatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Features
{
    public partial class MatchHub
    {
        [Authorize]
        public async Task CancelMatch(Guid matchId)
        {
            await _sender.Send(new CancelMatchCommand(matchId, Context.User?.Identity?.Name));

            await Clients.Group(matchId.ToString()).SendAsync("MatchCancelled", matchId);
        }
    }
}
