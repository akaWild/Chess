using Carter;
using MediatR;

namespace AuthService.Features.GetCurrentUser
{
    public class GetCurrentUserEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/auth/currentuser", async (ISender sender) =>
            {
                var result = await sender.Send(new GetCurrentUserQuery());

                return Results.Ok(result);
            }).RequireAuthorization();
        }
    }
}
