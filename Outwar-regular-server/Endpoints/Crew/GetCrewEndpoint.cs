using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.Items;

public static class GetCrewEndpoint
{
    public static IEndpointRouteBuilder MapGetCrewEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-crew", async (AppDbContext context, string crewName) =>
            {
            // Find the user in the database
            var crew = await context.Crews.Include(u => u.Members).FirstOrDefaultAsync(u => u.Name == crewName);
            if (crew == null)
            {
                return Results.NotFound($"Crew {crewName} not found.");
            }

            return Results.Ok(crew);
            })
            .WithName("GetCrew")
            .WithOpenApi();

        return app;
    }
}