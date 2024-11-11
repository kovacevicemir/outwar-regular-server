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

                itemToUpgrade.UpgradeLevel = itemToUpgrade.UpgradeLevel + 1;

                user.Items.Add(itemToUpgrade);

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