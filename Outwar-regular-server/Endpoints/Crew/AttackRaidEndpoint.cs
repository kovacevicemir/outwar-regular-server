using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using StackExchange.Redis;

namespace Outwar_regular_server.Endpoints.Items;

public static class AttackRaidEndpoint
{
    public static IEndpointRouteBuilder MapAttackRaidEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/attack-raid", async (AppDbContext context, IConnectionMultiplexer redis, string crewName, string raidName) =>
            {

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
                        var god = new God()
                        {
                            Name = "Rancid",
                            LevelRequirement = 21,
                            Attack = 10,
                            Hp = 50000,
                            Drops = ["Blood-Soaked Moccasins", "Ring of Hatred", "Blade of Dark Power"],
                            DropsChance = [15, 15, 15]
                        };
                    
                        var dropBags = DetermineDrops(god);
                        
                            if (dropBags.Count > 0)
                            {
                                var tasks = dropBags.Select(item =>
                                {
                                    // Add items to player asynchronously
                                    var addItemUrl = $"https://localhost:44338/add-item-to-user?username={deserializedRaid.CreatedBy}&itemName={item}";
                                    return client.PostAsync(addItemUrl, null); // Returns a Task
                                });
                                
                                // Await all the tasks to be completed
                                await Task.WhenAll(tasks);
                            
                                message += $" Drops: {string.Join(", ", dropBags)}";
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