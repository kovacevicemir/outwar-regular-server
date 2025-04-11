using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.Items;

public static class GetAllUserQuestsEndpoint
{
    public static IEndpointRouteBuilder MapGetAllUserQuestsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-all-quests", async (AppDbContext context, string username) =>
            {
            
                var user = await context.Users
                    .Include(u => u.Quests) // Eagerly load the Quests collection
                    .FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }

                return Results.Ok(user);
            })
            .WithName("GetAllUserQuests")
            .WithOpenApi();

        return app;
    }
}