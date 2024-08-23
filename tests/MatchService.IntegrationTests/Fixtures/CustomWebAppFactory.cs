using MassTransit;
using MatchService.Data;
using MatchService.IntegrationTests.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;
using WebMotions.Fake.Authentication.JwtBearer.Events;

namespace MatchService.IntegrationTests.Fixtures
{
    public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync().ConfigureAwait(false);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureTestServices(services =>
            {
                services.RemoveDbContext<DataContext>();
                services.AddDbContext<DataContext>(options =>
                {
                    options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
                });
                services.AddMassTransitTestHarness(opt =>
                {
                    opt.SetTestTimeouts(TimeSpan.FromSeconds(10));
                });
                services.EnsureCreated<DataContext>();
                services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme).AddFakeJwtBearer(opt =>
                {
                    opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;

                    opt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            StringValues accessToken = context.Request.Query["access_token"];

                            PathString path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/matches")))
                                context.Token = accessToken;

                            return Task.CompletedTask;
                        }
                    };
                });
            });
        }

        async Task IAsyncLifetime.DisposeAsync() => await _postgreSqlContainer.DisposeAsync().AsTask();
    }
}
