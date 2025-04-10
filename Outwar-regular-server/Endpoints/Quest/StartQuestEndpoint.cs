using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.Items;

public static class StartQuestEndpoint
{
    
    private static List<Quest>? quests;
    private static bool questsLoaded = false;
    
    public static IEndpointRouteBuilder MapStartQuestEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/start-quest", async (AppDbContext context, string username, string questName) =>
            {
                // Load quests only once
                if (!questsLoaded)
                {
                    var jsonFilePath = Path.Combine( "Data", "Quests.json");
                    try
                    {
                        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                        quests = await JsonSerializer.DeserializeAsync<List<Quest>>(stream) ?? new List<Quest>();
                        questsLoaded = true; // Set the flag to indicate items are loaded
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest($"Error reading the Quests.json file: {ex.Message}");
                    }
                }
            
                var user = await context.Users
                    .Include(u => u.Quests) // Eagerly load the user's Quests collection
                    .FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }

                if (user.Quests.Any(q => q.Name == questName))
                {
                    return Results.BadRequest("Quest already started by this user.");
                }
                
                // Verify quest list is loaded and not empty
                if (quests == null || !quests.Any())
                {
                    return Results.NotFound("Quests not found or the file is empty.");
                }
                
                var questToBeAdded = quests.FirstOrDefault(q => q.Name == questName);
                if (questToBeAdded == null)
                {
                    return Results.BadRequest($"Quest {questName} not found.");
                }
                
                user.Quests.Add(questToBeAdded);
                await context.SaveChangesAsync();

                return Results.Ok($"Quest {questName} added to user: {user.Name}");
            })
            .WithName("StartQuest")
            .WithOpenApi();

        return app;
    }
}