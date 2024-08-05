using AuthService.DTOs;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Features.Login
{
    public class LoginEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/login", async ([FromBody] LoginDto request, ISender sender) =>
            {
                var result = await sender.Send(new LoginCommand(request));

                return Results.Ok(result);
            });
        }
    }
}
