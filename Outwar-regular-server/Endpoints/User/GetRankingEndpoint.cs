using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.User;

public static class GetRankingEndpoint
{
    public static IEndpointRouteBuilder MapGetRanking(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-ranking", async (AppDbContext context) =>
            {
                var users = await context.Users
                    .Select(u => new
                    {
                        u.Name,
                        u.Level,
                        u.Experience
                    })
                    .OrderByDescending(u => u.Experience)
                    .ToListAsync();

                if (users == null)
                {
                    return Results.NotFound("Ranking not found.");
                }
                
                return Results.Ok(users);
            })
            .WithName("GetRanking")
            .WithOpenApi();

        return app;
    }
}