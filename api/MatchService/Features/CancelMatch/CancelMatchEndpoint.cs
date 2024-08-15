using MatchService.Exceptions;
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
            var username = Context.User?.Identity?.Name;
            if (username == null)
                throw new UserNotAuthenticated("User is not authenticated");

            await _sender.Send(new CancelMatchCommand(matchId, username));

            await Clients.Group(matchId.ToString()).SendAsync("MatchCancelled", matchId);
        }
    }
}
