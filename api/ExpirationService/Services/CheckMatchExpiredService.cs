using EventsLib;
using ExpirationService.Interfaces;
using MassTransit;

namespace ExpirationService.Services
{
    public class CheckMatchExpiredService : BackgroundService
    {
        private readonly IExpirationCollection _expirationService;
        private readonly IServiceProvider _services;

        public CheckMatchExpiredService(IExpirationCollection expirationService, IServiceProvider services)
        {
            _expirationService = expirationService;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckExpiredMatches();

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task CheckExpiredMatches()
        {
            var expiredMatches = _expirationService.GetExpiredMatches();
            if (expiredMatches.Length == 0)
                return;

            using var scope = _services.CreateScope();
            var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            foreach (var match in expiredMatches)
                await endpoint.Publish(new TimedOut(match.MatchId, match.SideToAct));
        }
    }
}
