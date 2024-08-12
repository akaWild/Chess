using ExpirationService.Consumers;
using ExpirationService.Interfaces;
using ExpirationService.Services;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<SideToActChangedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("exp", false));
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest")!);
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest")!);
        });
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddHostedService<CheckMatchExpiredService>();
builder.Services.AddSingleton<IExpirationCollectionService, ExpirationCollectionService>();

var host = builder.Build();

host.Run();

public partial class Program { }