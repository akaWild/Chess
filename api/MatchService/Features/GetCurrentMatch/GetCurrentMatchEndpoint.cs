using MatchService.Exceptions;
using MatchService.Features.GetCurrentMatch;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Features
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

            if (queryParam.Count != 0)
            {
                var matchId = queryParam[0]!;

                await Groups.AddToGroupAsync(Context.ConnectionId, matchId);

                if (!Guid.TryParse(matchId, out var matchIdGuid))
                    throw new MatchIdFormatException("Match Id has incorrect format");

                var result = await _sender.Send(new GetCurrentMatchQuery(matchIdGuid));

                await Clients.Caller.SendAsync("LoadMatchInfo", result);
            }
        }
    }
}
