using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Features.GetCurrentMatch
{
    public partial class MatchHub : Hub
    {
        private readonly ISender _sender;

        public MatchHub(ISender sender)
        {
            _sender = sender;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var httpContext = Context.GetHttpContext();
            if (httpContext == null)
                return;

            var queryParam = httpContext.Request.Query["matchId"];

            if (queryParam.Count == 0)
                throw new HubException("Match Id wasn't provided");

            var matchId = queryParam[0]!;

            await Groups.AddToGroupAsync(Context.ConnectionId, matchId);

            if (!Guid.TryParse(matchId, out var matchIdGuid))
                throw new HubException("Match Id has incorrect format");

            try
            {
                var result = await _sender.Send(new GetCurrentMatchQuery(matchIdGuid));

                await Clients.Caller.SendAsync("LoadMatchInfo", result);
            }
            catch (Exception e)
            {
                throw new HubException(e.Message);
            }
        }
    }
}
