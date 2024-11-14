using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.Items;

public static class GetAllUserQuestsEndpoint
{
    public static IEndpointRouteBuilder MapGetAllUserQuestsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-all-quests", async (AppDbContext context, string username) =>
            {
            
                var user = await context.Users
                    .Include(u => u.Quests) // Eagerly load the user's Items collection
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