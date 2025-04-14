using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints;

public static class GetUserLocationEndpoint
{
    public static IEndpointRouteBuilder MapGetUserLocation(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-user-location", async (AppDbContext context, string username) =>
            {
                // Fetch the user location
                var location = await context.Users
                    .Where(u => u.Name == username)
                    .Select(u => u.Location)
                    .FirstOrDefaultAsync();

                if (location == null)
                {
                    return Results.NotFound("User location not found.");
                }

                return Results.Ok(location);
            })
            .WithName("GetUserLocation")
            .WithOpenApi();

        return app;
    }
}