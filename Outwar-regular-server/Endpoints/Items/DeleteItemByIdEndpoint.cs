using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.Items;

public static class DeleteItemByIdEndpoint
{
    private static List<Item>? items;
    private static bool itemsLoaded = false;
    public static IEndpointRouteBuilder MapDeleteItemByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/delete-item-by-id", async (AppDbContext context, string username, int itemId) =>
            {
                // Find the item by ID
                var item = await context.Items.FirstOrDefaultAsync(x => x.Id == itemId);
                if (item == null)
                {
                    return Results.NotFound("Item for delete not found");
                }

                // Find the user in the database
                var user = await context.Users
                    .Include(u => u.Items)          // Ensure Items are included if you need them
                    .Include(u => u.EquipedItemsId) // Include Equip Items
                    .FirstOrDefaultAsync(u => u.Name == username);

                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }

                // Remove the item from user's EquipItemsId list (if it exists)
                var equippedItem = user.EquipedItemsId.FirstOrDefault(x => x == itemId);
                if (equippedItem != null)
                {
                    user.EquipedItemsId.Remove(equippedItem);
                }

                // Remove the item from the Items table
                context.Items.Remove(item);

                // Save changes to the database
                await context.SaveChangesAsync();

                return Results.Ok($"Item {itemId} has been deleted for user {username}.");
            })
            .WithName("DeleteItemById")
            .WithOpenApi();

        return app;
    }
}