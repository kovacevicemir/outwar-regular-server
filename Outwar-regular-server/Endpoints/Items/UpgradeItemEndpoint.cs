using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.Items;

public static class UpgradeItemEndpoint
{
    public static IEndpointRouteBuilder MapUpgradeItemLevelByItemId(this IEndpointRouteBuilder app)
    {
        app.MapPost("/upgrade-item-level-by-item-id", async (AppDbContext context, string username, int itemId) =>
            {
                var user = await context.Users
                    .Include(u => u.Items) // Eagerly load the user's Items collection
                    .FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }

                var itemToUpgrade = user.Items.FirstOrDefault(i => i.Id == itemId);
                if (itemToUpgrade == null)
                {
                    return Results.NotFound($"Item with ID {itemId} not found for user {username}.");
                }

                if (itemToUpgrade.UpgradeLevel > 2)
                {
                    return Results.BadRequest("Item is already maximum upgrade level.");
                }

                var pointsNeeded = 0;
                switch (itemToUpgrade.UpgradeLevel)
                {
                    case 0:
                        pointsNeeded = 20;
                        break;
                    case 1:
                        pointsNeeded = 30;
                        break;
                    case 2:
                        pointsNeeded = 50;
                        break;
                }

                if (user.Points < pointsNeeded)
                {
                    return Results.BadRequest("Not enough points.");
                }


                var upgradeMultiplier = 0.0;
                switch (itemToUpgrade.UpgradeLevel)
                {
                    case 0:
                        upgradeMultiplier = 0.1;
                        break;
                    case 1:
                        upgradeMultiplier = 0.15;
                        break;
                    case 2:
                        upgradeMultiplier = 0.2;
                        break;
                }
                
                itemToUpgrade.UpgradeLevel = itemToUpgrade.UpgradeLevel + 1;


                for (int i = 0; i < itemToUpgrade.Stats.Length; i++)
                {
                    itemToUpgrade.Stats[i] = (int)Math.Round(itemToUpgrade.Stats[i] + (itemToUpgrade.Stats[i] * upgradeMultiplier));
                }

                user.Items.Add(itemToUpgrade);
                user.Points -= pointsNeeded;

                // Save changes to the database
                await context.SaveChangesAsync();

                return Results.Ok(
                    $"Item with ID {itemId} - {itemToUpgrade.Name} successfully upgraded lvl-{itemToUpgrade.UpgradeLevel} for user {username}.");
            })
            .WithName("UpgradeItemByItemId")
            .WithOpenApi();

            return app;
    }
}