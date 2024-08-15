using FluentValidation;
using MatchService.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Utils
{
    public class MatchHubCustomFilter : IHubFilter
    {
        public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            try
            {
                return await next(invocationContext);
            }
            catch (Exception e) when (e is BaseClientException or ValidationException)
            {
                await invocationContext.Hub.Clients.Caller.SendAsync("ClientError", e.Message);

                throw;
            }
            catch (Exception e)
            {
                await invocationContext.Hub.Clients.Caller.SendAsync("ServerError", "Something went wrong");

                throw;
            }
        }
    }
}
