using MatchService.DTOs;
using MatchService.Features.CreateMatch;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Features
{
    public partial class MatchHub
    {
        public async Task CreateMatch(CreateMatchDto createMatchDto)
        {
            try
            {
                var result = await _sender.Send(new CreateMatchCommand(createMatchDto));

                await Groups.AddToGroupAsync(Context.ConnectionId, result.MatchId.ToString());

                await Clients.Caller.SendAsync("MatchCreated", result);
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync("Error", e.Message);
            }
        }
    }
}
