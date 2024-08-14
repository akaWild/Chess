using MatchService.Data;
using MatchService.Features.GetCurrentMatch;
using MatchService.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSignalR();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();

var app = builder.Build();

app.MapHub<MatchHub>("matches");

DbInitializer.InitDb(app);

app.Run();
