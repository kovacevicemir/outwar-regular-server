using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using StackExchange.Redis;

namespace Outwar_regular_server.Endpoints.Items;

public static class IncreaseSkillLevelEndpoint
{
    private static List<Skill>? allSkills;
    private static bool skillsLoaded = false;

    public static IEndpointRouteBuilder MapIncreaseSkillLevelEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/increase-skill-level",
                async (AppDbContext context, IConnectionMultiplexer redis, string userName, string skillName) =>
                {
                    if (!skillsLoaded)
                    {
                        var jsonFilePath = @"Data\Skills.json";
                        try
                        {
                            using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                            allSkills = await JsonSerializer.DeserializeAsync<List<Skill>>(stream) ?? new List<Skill>();
                            skillsLoaded = true; // Set the flag to indicate skills are loaded
                        }
                        catch (Exception ex)
                        {
                            return Results.BadRequest($"Error reading the Skills.json file: {ex.Message}");
                        }
                    }

                    // Find the user in the database
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Name == userName);
                    if (user == null)
                    {
                        return Results.NotFound($"User {userName} not found.");
                    }
                    
                    //Check if enough sp
                    if (user.SkillPoints < 1)
                    {
                        return Results.Ok($"User {userName} needs a skill points.");
                    }
                    
                    

                    var skillIndex = allSkills.FindIndex(skill => skill.Name == skillName);
                    if (skillIndex == -1)
                    {
                        return Results.BadRequest($"Skill {skillName} not found.");
                    }
                    
                    //Check if skill is already level 10
                    if (user.Skills[skillIndex] > 9)
                    {
                        return Results.BadRequest($"{skillName} is already max level!");
                    }
                    
                    user.Skills[skillIndex] += 1;
                    user.SkillPoints -= 1;

                    await context.SaveChangesAsync();

                    return Results.Ok($"{skillName} level increased {user.Skills[skillIndex]}.");
                })
            .WithName("IncreaseSkillLevel")
            .WithOpenApi();

        return app;
    }
}