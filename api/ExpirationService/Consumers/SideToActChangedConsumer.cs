using EventsLib;
using ExpirationService.Interfaces;
using ExpirationService.Models;
using MassTransit;

namespace ExpirationService.Consumers
{
    public class SideToActChangedConsumer : IConsumer<SideToActChanged>
    {
        private readonly IExpirationCollectionService _expirationService;

        public SideToActChangedConsumer(IExpirationCollectionService expirationService)
        {
            _expirationService = expirationService;
        }

        public Task Consume(ConsumeContext<SideToActChanged> context)
        {
            _expirationService.Remove(context.Message.MatchId);
            _expirationService.Add(new MatchExpirationInfo(context.Message.MatchId, context.Message.SideToAct, context.Message.ExpTimeUtc));

            return Task.CompletedTask;
        }
    }
}
