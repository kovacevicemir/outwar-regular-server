using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.User;

public static class GetUserByUsernameEndpoint
{
    public static IEndpointRouteBuilder MapGetUserByUsername(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-user-by-username", async (AppDbContext context, string username) =>
            {
                // Fetch the user along with their Items and Quests
                var user = await context.Users
                    .Include(u => u.Items)      // Include the user's Items
                    .Include(u => u.Quests)     // Include the user's Quests
                    .FirstOrDefaultAsync(u => u.Name == username);

                if (user == null)
                {
                    return Results.NotFound("User not found.");
                }

                // Combine all MonsterIds from all Quests into a single list
                var allMonsterIds = user.Quests
                    .SelectMany(quest => quest.MonsterIds)  // Flatten the collection of MonsterIds
                    .Distinct()                             // Remove duplicates, if needed
                    .ToList();                              // Convert to a list

                // You can now return the user along with the combined list of MonsterIds
                var userWithMonsterIds = new
                {
                    user.Id,
                    user.Name,
                    user.Level,
                    user.Rage,
                    user.RagePerHour,
                    user.Experience,
                    user.Items,
                    user.EquipedItemsId,
                    user.Location,
                    QuestMonsterIds = allMonsterIds, // Return the combined list of MonsterIds
                    user.CrewName
                };

                return Results.Ok(userWithMonsterIds);
            })
            .WithName("GetUserByUsername")
            .WithOpenApi();

        return app;
    }
}