using MatchService.Features.AcceptMatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Features
{
    public partial class MatchHub
    {
        [Authorize]
        public async Task AcceptMatch(Guid matchId)
        {
            await _sender.Send(new AcceptMatchCommand(matchId, Context.User?.Identity?.Name));

            await Clients.Group(matchId.ToString()).SendAsync("MatchStarted", matchId);
        }
    }
}
