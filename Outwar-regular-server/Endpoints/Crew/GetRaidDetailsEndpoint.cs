using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using StackExchange.Redis;

namespace Outwar_regular_server.Endpoints.Items;

public static class GetRaidDetailsEndpoint
{
    public static IEndpointRouteBuilder MapGetRaidDetailsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-raid-details", async (AppDbContext context, IConnectionMultiplexer redis, string crewName, string raidName) =>
            {
            
            var db = redis.GetDatabase();
            var jsonValue = await db.StringGetAsync($"raid-{crewName}-{raidName}");
            
            if (jsonValue.IsNullOrEmpty)
                return Results.NotFound($"Key raid-{crewName}-{raidName} not found");
            
            var deserializedRaid = JsonSerializer.Deserialize<Raid>(jsonValue);

            return Results.Ok(deserializedRaid);
            })
            .WithName("GetRaidDetails")
            .WithOpenApi();

        return app;
    }
}