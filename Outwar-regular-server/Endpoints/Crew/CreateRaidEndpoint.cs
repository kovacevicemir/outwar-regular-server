using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using StackExchange.Redis;

namespace Outwar_regular_server.Endpoints.Items;

public static class CreateRaidEndpoint
{
    public static IEndpointRouteBuilder MapCreateRaidEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/create-raid", async (AppDbContext context, IConnectionMultiplexer redis, string crewName, string createdBy, string raidName) =>
            {
            // Find the user in the database
            var user = await context.Users.FirstOrDefaultAsync(u => u.Name == createdBy);
            if (user == null)
            {
                return Results.NotFound($"User {createdBy} not found.");
            }
            
            var db = redis.GetDatabase();
            
            //Form raid:
            var raid = new Raid()
            {
                Id = 1,
                RaidName = raidName,
                RaidMembers = [user],
                CreatedBy = user,
                HpLeft = 50000 //Hard coded - to do later per raid...
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