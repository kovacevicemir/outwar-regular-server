using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using StackExchange.Redis;

namespace Outwar_regular_server.Endpoints.Items;

public static class AttackRaidEndpoint
{
    private static List<God>? gods;
    private static bool godsLoaded = false;
    public static IEndpointRouteBuilder MapAttackRaidEndpoint(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapPost("/attack-raid", async (AppDbContext context, IConnectionMultiplexer redis, string crewName, string raidName) =>
            {
                
                if (!godsLoaded)
                {
                    var jsonFilePath = Path.Combine( "Data", "Gods.json");
                    try
                    {
                        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                        gods = await JsonSerializer.DeserializeAsync<List<God>>(stream) ?? new List<God>();
                        godsLoaded = true; // Set the flag to indicate items are loaded

                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest($"Error reading the Gods.json file: {ex.Message}");
                    }
                }
                
                if (gods == null || !gods.Any())
                {
                    return Results.NotFound("Gods not found or the file is empty.");
                }
                
                var god = gods.SingleOrDefault(g => g.Name == raidName);
                if (god == null)
                {
                    return Results.NotFound($"God - {raidName} not found or the file is empty.");
                }

                var db = redis.GetDatabase();
                var jsonValue = await db.StringGetAsync($"raid-{crewName}-{raidName}");
            
                if (jsonValue.IsNullOrEmpty)
                    return Results.NotFound($"Key raid-{crewName}-{raidName} not found");
            
                var deserializedRaid = JsonSerializer.Deserialize<Raid>(jsonValue);

                deserializedRaid.HpLeft = deserializedRaid.HpLeft - 10000; //decrease 10k hp hard coded... to do later

                var message = $"Attack {raidName} done! ";

                // Check if raid is done - hardcoded for now Rancid
                if(deserializedRaid.HpLeft <= 0)
                {
                    using (var client = new HttpClient()){
                        var dropBags = DetermineDrops(god);
                        
                            if (dropBags.Count > 0)
                            {
                                var tasks = dropBags.Select(item =>
                                {
                                    // Add items to player asynchronously
                                    var addItemUrl = $"{config["BaseUrl:BackendUrl"]}/add-item-to-user?username={deserializedRaid.CreatedBy.Name}&itemName={item}";
                                    return client.PostAsync(addItemUrl, null); // Returns a Task
                                });
                                
                                // Await all the tasks to be completed
                                await Task.WhenAll(tasks);
                            
                                message += $" Drops: {string.Join(", ", dropBags)}";
                            }
                            else //Reward some points to user who created raid - if no drops
                            {
                                Random rnd = new Random();
                                var pointsToAdd = rnd.Next(1, 10); 
                                
                                var user = context.Users.FirstOrDefault(u => u.Name == deserializedRaid.CreatedBy.Name);
                                if (user != null)
                                {
                                    user.Points += pointsToAdd;
                                    await context.SaveChangesAsync();
                                    message += $" Points: {pointsToAdd}";
                                }
                            }
                    }
                    
                    //Delete raids from redis
                    var isDeleted = await db.KeyDeleteAsync($"raid-{crewName}-{raidName}");
                    Console.WriteLine(isDeleted);
                }
                else
                {
                    message += $" Hp left: {deserializedRaid.HpLeft}";
                    var jsonRaid = JsonSerializer.Serialize(deserializedRaid);
                    await db.StringSetAsync($"raid-{crewName}-{raidName}", jsonRaid);
                }

                return Results.Ok(message);
            })
            .WithName("AttackRaid")
            .WithOpenApi();

        return app;
    }
    
    public static List<string> DetermineDrops(God monster)
    {
        var dropBag = new List<string>();

        // Ensure we have valid data for Drops and DropsChance
        if (monster.Drops == null || monster.DropsChance == null || monster.Drops.Count != monster.DropsChance.Count)
        {
            Console.WriteLine("Invalid data: Drops and DropsChance must be of the same length.");
            return dropBag;
        }

        // Loop over each drop and corresponding chance
        for (int i = 0; i < monster.Drops.Count; i++)
        {
            var dropChance = monster.DropsChance[i];
            var dropItem = monster.Drops[i];

            // Generate a random number between 1 and 100
            var randomNumber = new Random().Next(1, 101);  // Random number between 1 and 100

            // If the random number is less than or equal to the dropChance, the item drops
            if (randomNumber <= dropChance)
            {
                dropBag.Add(dropItem);  // Add the drop item to the dropBag
            }
        }

        return dropBag;
    }
}