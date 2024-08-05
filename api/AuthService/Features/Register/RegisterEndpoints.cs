using AuthService.DTOs;
using Carter;
using MediatR;

namespace AuthService.Features.Register
{
    public class RegisterEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/register", async (RegisterDto request, ISender sender) =>
            {
                var result = await sender.Send(new RegisterCommand(request));

                return Results.Ok(result);
            });
        }
    }
}
