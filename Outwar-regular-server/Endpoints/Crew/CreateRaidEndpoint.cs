using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using StackExchange.Redis;

namespace Outwar_regular_server.Endpoints.Items;

public static class CreateRaidEndpoint
{
    private static List<God>? gods;
    private static bool godsLoaded = false; // Flag to indicate if gods have been loaded
    public static IEndpointRouteBuilder MapCreateRaidEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/create-raid", async (AppDbContext context, IConnectionMultiplexer redis, string crewName, string createdBy, string raidName) =>
            {
                // Find the user in the database
                var user = await context.Users.Include(u => u.Items).FirstOrDefaultAsync(u => u.Name == createdBy);
                if (user == null)
                {
                    return Results.NotFound($"User {createdBy} not found.");
                }

                if (!godsLoaded)
                {
                    var jsonFilePath = Path.Combine("Data", "Gods.json");
                    try
                    {
                        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                        gods = await JsonSerializer.DeserializeAsync<List<God>>(stream) ?? new List<God>();
                        godsLoaded = true; // Set the flag to true after loading
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest($"Error reading the Gods.json file: {ex.Message}");
                    }
                }

                if (gods == null || !gods.Any())
                {
                    return Results.NotFound("God not found or the file is empty.");
                }

                //Find god details
                var godDetails = gods.SingleOrDefault(g => g.Name == raidName);
                if (godDetails == null)
                {
                    return Results.BadRequest("Could not found godDetails. Either god name does not exists, or problem in json file.");
                }

                var db = redis.GetDatabase();

                //Check rage
                if (user.Rage <= 50)
                {
                    return Results.Ok("You dont have enough rage to form raid! 50 rage minimum");
                }
                else
                {
                    user.Rage -= 50;
                    await context.SaveChangesAsync();
                }

                //Form raid:
                var raid = new Raid()
                {
                    Id = 1,
                    RaidName = raidName,
                    RaidMembers = [user],
                    CreatedBy = user,
                    HpLeft = godDetails.Hp
                };

                var jsonRaid = JsonSerializer.Serialize(raid);

                await db.StringSetAsync($"raid-{crewName}-{raidName}", jsonRaid);

                return Results.Ok($" ${user.Name}");
            })
            .WithName("CreateRaid")
            .WithOpenApi();

        return app;
    }
}