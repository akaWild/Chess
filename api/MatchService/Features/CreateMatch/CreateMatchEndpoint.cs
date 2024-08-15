using MatchService.DTOs;
using MatchService.Features.AcceptMatch;
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
            var createMatchResult = await _sender.Send(new CreateMatchCommand(createMatchDto, Context.User?.Identity?.Name));

            await Groups.AddToGroupAsync(Context.ConnectionId, createMatchResult.MatchId.ToString());

            await Clients.Caller.SendAsync("MatchCreated", createMatchResult);

            if (createMatchDto.VsBot)
            {
                var acceptMatchResult = await _sender.Send(new AcceptMatchCommand(createMatchDto.MatchId, "Bot"));

                await Clients.Caller.SendAsync("MatchStarted", acceptMatchResult);
            }
        }
    }
}
