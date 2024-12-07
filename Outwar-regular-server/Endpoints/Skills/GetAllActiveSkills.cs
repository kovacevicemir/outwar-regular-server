using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using Outwar_regular_server.Utilities;
using StackExchange.Redis;

namespace Outwar_regular_server.Endpoints.Items;

public static class GetAllActiveSkillsEndpoint
{
    // private static List<Skill>? allSkills;
    // private static bool skillsLoaded = false;

    public static IEndpointRouteBuilder MapGetAllActiveSkillsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-all-active-skills",
                async (AppDbContext context, IConnectionMultiplexer redis, string userName) =>
                {
                    var buffManager = new BuffManager(redis);

                    // Find all buffs
                    var activeBuffs = buffManager.GetActiveBuffs(userName);

                    // Filter out buffs with a duration less than 23 hours
                    // var filteredBuffs = activeBuffs
                    //     .Where(buff => buff.Value.TimeRemaining >= TimeSpan.FromHours(23));

                    // Transform the dictionary into a list of anonymous objects
                    var buffsList = activeBuffs.Select(buff => new
                    {
                        SkillName = buff.Key,
                        Duration = buff.Value.TimeRemaining.ToString("hh\\:mm\\:ss"), // Format the TimeSpan
                        Bonus = buff.Value.BonusValue
                    }).ToList();

                    return Results.Ok(buffsList);
                })
            .WithName("GetAllActiveSkills")
            .WithOpenApi();

        return app;
    }
}