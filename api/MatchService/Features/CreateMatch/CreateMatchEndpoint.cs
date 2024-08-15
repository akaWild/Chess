using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Features.CreateMatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Features
{
    public partial class MatchHub
    {
        [Authorize]
        public async Task CreateMatch(CreateMatchDto createMatchDto)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
                throw new UserNotAuthenticated("User is not authenticated");

            var result = await _sender.Send(new CreateMatchCommand(createMatchDto, username));

            await Groups.AddToGroupAsync(Context.ConnectionId, result.MatchId.ToString());

            await Clients.Caller.SendAsync("MatchCreated", result);
        }
    }
}
