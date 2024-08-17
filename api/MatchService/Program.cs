using FluentValidation;
using MatchService.Data;
using MatchService.Extensions;
using MatchService.Features;
using MatchService.Features.CreateMatch;
using MatchService.Interfaces;
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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSignalR().AddHubOptions<MatchHub>(options =>
{
    options.AddFilter<MatchHubCustomFilter>();
});
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddScoped<IMatchRepository, MatchRepository>();

var app = builder.Build();

app.MapHub<MatchHub>("/matches");

DbInitializer.InitDb(app);

app.Run();

public partial class Program { }