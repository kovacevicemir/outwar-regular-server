using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.Items;

public static class DeleteAllFromInventoryEndpoint
{
    public static IEndpointRouteBuilder MapDeleteAllFromInventoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/delete-all-from-inventory", async (AppDbContext context, string username) =>
            {
                var user = await context.Users
                    .Include(u => u.Items) // Eagerly load the user's Items collection
                    .FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }

                var itemsThatAreNotLocked = user.Items.Where(i => i.Locked == false); //If item is locked (Lock = true on item) dont delete it.
                var itemsToRemove = itemsThatAreNotLocked.Where(i => !user.EquipedItemsId.Contains(i.Id)); //filter equiped items !IMPORTANT
                foreach (var item in itemsToRemove)
                {
                    user.Items.Remove(item);
                    context.Items.Remove(item);
                }

                // Save changes to the database
                await context.SaveChangesAsync();

                return Results.Ok($"Items deleted successfully!");
            })
            .WithName("DeleteAllFromInventory")
            .WithOpenApi();

        return app;
    }
}