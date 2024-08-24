using FluentValidation;
using MassTransit;
using MatchService.Data;
using MatchService.Extensions;
using MatchService.Features;
using MatchService.Features.CreateMatch;
using MatchService.Interfaces;
using MatchService.Services;
using MatchService.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SharedLib.Behaviors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining(typeof(CreateMatchCommand));
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<DataContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);

        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("match", false));
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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSignalR().AddHubOptions<MatchHub>(options =>
{
    options.AddFilter<MatchHubCustomFilter>();
});
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<ILocalExpirationService, LocalExpirationService>();

var app = builder.Build();

app.MapHub<MatchHub>("/matches");

DbInitializer.InitDb(app);

app.Run();

public partial class Program { }