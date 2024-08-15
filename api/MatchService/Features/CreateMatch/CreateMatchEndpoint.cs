using MatchService.DTOs;
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
            var result = await _sender.Send(new CreateMatchCommand(createMatchDto, Context.User?.Identity?.Name));

            await Groups.AddToGroupAsync(Context.ConnectionId, result.MatchId.ToString());

            await Clients.Caller.SendAsync("MatchCreated", result);
        }
    }
}
