namespace ExpirationService.Services
{
    public class CheckExpirationService : BackgroundService
    {
        public CheckExpirationService()
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
