using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.User;

public static class IncreaseExpEndpoint
{
    private static List<ExperienceListItem> experienceList;
    private static bool experienceListLoaded = false;
    public static IEndpointRouteBuilder MapIncreaseExp(this IEndpointRouteBuilder app)
    {
        app.MapPost("/increase-exp", async (AppDbContext context, string username, int exp) =>
            {
                // Load experience list only once
                if (!experienceListLoaded)
                {
                    var jsonFilePath = @"Data\experienceList.json";
                    try
                    {
                        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                        experienceList = await JsonSerializer.DeserializeAsync<List<ExperienceListItem>>(stream) ?? new List<ExperienceListItem>();
                        experienceListLoaded = true; // Set flag to prevent reloading ExperienceListItem
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading the experienceList.json file: {ex.Message}");
                        return Results.BadRequest("Error loading experience list.");
                    }
                }

                // Verify experience list has been loaded successfully
                if (experienceList == null || !experienceList.Any())
                {
                    return Results.NotFound("Experience list not found or empty.");
                }

                // Find the user
                var user = await context.Users.FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }

                // Update experience points
                user.Experience += exp;

                // Check if the user levels up
                bool isLevelUp = false;

                var nextLevel = experienceList[user.Level];
                if (nextLevel != null)
                {
                    if (user.Experience >= nextLevel.Experience)
                    {
                        user.Level += 1;
                        isLevelUp = true;
                    }
                }
                
                // if (experienceList.TryGetValue(user.Level + 1, out int expNeededForLevel) && user.Experience >= expNeededForLevel)
                // {
                //     user.Level += 1;
                //     isLevelUp = true;
                // }

                await context.SaveChangesAsync();

                return Results.Ok($"Added {exp} experience to {username}. Level up: {isLevelUp}");
            })
            .WithName("IncreaseExp")
            .WithOpenApi();

        return app;
    }
}