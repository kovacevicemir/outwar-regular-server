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

                // call increase Exp endpoint here
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
                }
                

                return Results.Ok($"{monster.Name} is killed! ToDo: add combat and reward logic later");
            })
            .WithName("AttackMonsterByName")
            .WithOpenApi();

        return app;
    }
}