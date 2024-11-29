using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.Items;

public static class GetSingleQuestEndpoint
{
    public static IEndpointRouteBuilder MapGetSingleQuestEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-single-quest", async (AppDbContext context, string username, string questName) =>
            {
                var user = await context.Users
                    .Include(u => u.Quests) // Eagerly load the user's Items collection
                    .FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }
                
                var quest = user.Quests.FirstOrDefault(q => q.Name == questName);
                if (quest == null)
                {
                    return Results.NotFound($"Quest {questName} not found.");
                }

                return Results.Ok(quest);
            })
            .WithName("GetSingleQuest")
            .WithOpenApi();

        return app;
    }
}