using ExpirationService.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<CheckExpirationService>();

var host = builder.Build();

host.Run();
