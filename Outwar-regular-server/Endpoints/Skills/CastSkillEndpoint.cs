using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using Outwar_regular_server.Utilities;
using StackExchange.Redis;

namespace Outwar_regular_server.Endpoints.Items;

public static class CastSkillEndpoint
{
    private static List<Skill>? allSkills;
    private static bool skillsLoaded = false;

    public static IEndpointRouteBuilder MapCastSkillEndpoint(this IEndpointRouteBuilder app)
    {
        
        app.MapPost("/cast-skill",
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
                    
                    var buffManager = new BuffManager(redis);
                    
                    // Find buff with name - if already casted
                    var activeBuffs = buffManager.GetActiveBuffs(userName);
                    var targetSkillPair = activeBuffs.FirstOrDefault(skill => skill.Key == skillName);

                    // Check if the KeyValuePair is valid
                    if (!targetSkillPair.Equals(default(KeyValuePair<string, (int, TimeSpan)>)))
                    {
                        var expirationTime = targetSkillPair.Value.Item2; // Access the TimeSpan (ExpirationTime)
                        return Results.BadRequest($"Skill {skillName} is already in use. Cooldown time is {expirationTime}");
                    }
                    
                    // Find the user in the database
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Name == userName);
                    if (user == null)
                    {
                        return Results.NotFound($"User {userName} not found.");
                    }

                    // Find skill bonus, level etc
                    var skillDefinition = allSkills.FirstOrDefault(s => s.Name == skillName);
                    var skillIndex = allSkills.FindIndex(s => s.Name == skillName);
                    var userSkillLevel = user.Skills[skillIndex];
                    if (userSkillLevel == 0)
                    {
                        return Results.BadRequest($"Skill {skillName} is not trained yet!");                        
                    }
                    
                    var bonusValue = skillDefinition.LevelValues[userSkillLevel];
                    
                    // Cast a buff
                    buffManager.CastBuff(userName, skillName, bonusValue, TimeSpan.FromHours(24));
                    
                    return Results.Ok($"{skillName} casted! Time remaining 60 minutes!");
                })
            .WithName("CastSkill")
            .WithOpenApi();

        return app;
    }
}