using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.User;

public static class AttackMonsterByNameEndpoint
{
    private static List<Monster>? monsters;
    private static bool monstersLoaded = false; // Flag to indicate if monsters have been loaded

    public static IEndpointRouteBuilder MapAttackMonsterByName(this IEndpointRouteBuilder app)
    {
        app.MapPost("/attack-monster-by-name", async (AppDbContext context, string monsterName, string username) =>
            {
                if (!monstersLoaded)
                {
                    var jsonFilePath = @"Data\Monsters.json";
                    try
                    {
                        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                        monsters = await JsonSerializer.DeserializeAsync<List<Monster>>(stream) ?? new List<Monster>();
                        monstersLoaded = true; // Set the flag to true after loading
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest($"Error reading the Monsters.json file: {ex.Message}");
                    }
                }

                if (monsters == null || !monsters.Any())
                {
                    return Results.NotFound("Monsters not found or the file is empty.");
                }
            
                var monster = monsters.FirstOrDefault(m => m.Name.Equals(monsterName, StringComparison.OrdinalIgnoreCase));
                if (monster == null)
                {
                    return Results.NotFound("Monster not found. Please check the monster name.");
                }
                
                var message = $"{monster.Name} is killed! Exp: {monster.Exp}";

                // call increase Exp endpoint here && call quest progress TODO
                using (var client = new HttpClient())
                {
                    var user = await context.Users
                        .FirstOrDefaultAsync(u => u.Name == username);
                    if (user == null)
                    {
                        return Results.NotFound($"User {username} not found.");
                    }
                    
                    var increaseExpUrl = $"https://localhost:44338/increase-exp?username={user.Name}&exp={monster.Exp}";
                    var response = await client.PostAsync(increaseExpUrl, null);

                    if (!response.IsSuccessStatusCode)
                    {
                        return Results.BadRequest("Error requesting increase-exp inside AttackMonsterByName endpoint.");
                    }
                    
                
                    var dropBags = DetermineDrops(monster);
                    if (dropBags.Count > 0)
                    {
                        var tasks = dropBags.Select(item =>
                        {
                            // Add items to player asynchronously
                            var addItemUrl = $"https://localhost:44338/add-item-to-user?username={username}&itemName={item}";
                            return client.PostAsync(addItemUrl, null); // Returns a Task
                        });
                        
                        // Await all the tasks to be completed
                        await Task.WhenAll(tasks);
                    
                        message += $" Drops: {string.Join(", ", dropBags)}";
                    }
                }

                return Results.Ok(message);
            })
            .WithName("AttackMonsterByName")
            .WithOpenApi();

        return app;
    }
    
    public static List<string> DetermineDrops(Monster monster)
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