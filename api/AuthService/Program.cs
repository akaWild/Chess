using AuthService.Data;
using AuthService.Extensions;
using AuthService.Features.Register;
using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedLib.Behaviors;
using SharedLib.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCarter();
builder.Services.AddValidatorsFromAssemblyContaining(typeof(RegisterCommand));
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.UseExceptionHandler(options => { });

await DbInitializer.InitDb(app);

app.Run();
