using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using StackExchange.Redis;

namespace Outwar_regular_server.Endpoints.Items;

public static class GetCrewRaidsEndpoint
{
    public static IEndpointRouteBuilder MapCrewRaidsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-crew-raids", async (AppDbContext context, IConnectionMultiplexer redis, string crewName) =>
            {
                var db = redis.GetDatabase();
                var server = redis.GetServer(redis.GetEndPoints().First());
        
                // Use SCAN to efficiently find keys matching the pattern
                var keys = server.Keys(pattern: $"raid-{crewName}-*").ToList();
        
                if (!keys.Any())
                    return Results.NotFound($"No keys found for raid-{crewName}-*");
        
                var raids = new List<Raid>();
        
                foreach (var key in keys)
                {
                    var jsonValue = await db.StringGetAsync(key);
                    if (!jsonValue.IsNullOrEmpty)
                    {
                        var raid = JsonSerializer.Deserialize<Raid>(jsonValue);
                        if (raid != null)
                            raids.Add(raid);
                    }
                }

                return Results.Ok(raids);
            })
            .WithName("CrewRaids")
            .WithOpenApi();

        return app;
    }
}