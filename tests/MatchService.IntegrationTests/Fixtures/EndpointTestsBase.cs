using AutoFixture;
using MassTransit.Testing;
using MatchService.Data;
using MatchService.IntegrationTests.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace MatchService.IntegrationTests.Fixtures
{
    public class EndpointTestsBase : IAsyncLifetime
    {
        protected readonly Fixture Fixture;
        protected readonly CustomWebAppFactory Factory;
        protected readonly HttpMessageHandler HttpMessageHandler;

        protected readonly ITestHarness Harness;

        protected bool ResponseReceived = false;
        protected string? ClientErrorMessage;
        protected string? ServerErrorMessage;

        public EndpointTestsBase(CustomWebAppFactory factory)
        {
            Fixture = new Fixture();
            Factory = factory;
            HttpMessageHandler = Factory.Server.CreateHandler();
            Harness = Factory.Services.GetTestHarness();
        }

        public virtual Task InitializeAsync()
        {
            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<DataContext>();

            DbHelper.ReinitDbForTests(db);

            return Task.CompletedTask;
        }

        public virtual Task DisposeAsync()
        {
            ResponseReceived = false;
            ClientErrorMessage = null;
            ServerErrorMessage = null;

            return Task.CompletedTask;
        }

        protected virtual void SetConnectionHandlers(HubConnection hubConnection)
        {
            hubConnection.On<string, string>("ClientError", (errMsg, _) =>
            {
                ClientErrorMessage = errMsg;

                ResponseReceived = true;
            });
            hubConnection.On<string>("ServerError", (errMsg) =>
            {
                ServerErrorMessage = errMsg;

                ResponseReceived = true;
            });
        }

        protected Task WaitForResponse(int timeout)
        {
            var startTime = DateTime.Now;
            while (!ResponseReceived)
            {
                if (DateTime.Now - startTime > TimeSpan.FromMilliseconds(timeout))
                    break;

                Thread.Sleep(10);
            }

            return Task.CompletedTask;
        }
    }
}
