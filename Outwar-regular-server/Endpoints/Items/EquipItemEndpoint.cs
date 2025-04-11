using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.Items;

public static class EquipItemEndpoint
{
    public static IEndpointRouteBuilder MapEquipItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/equip-item", async (AppDbContext context, string username, int itemId) =>
            {
            
                var user = await context.Users
                    .Include(u => u.Items) // Eagerly load the user's Items collection
                    .FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }
                
                //Check if item is already equiped for some reason.
                if (user.EquipedItemsId.Any(equipedItemId => equipedItemId == itemId))
                {
                    return Results.StatusCode(StatusCodes.Status409Conflict);
                }
                
                //Check if itemId exists in this current user - prevent injecting random ID
                if (!user.Items.Any(item => item.Id == itemId))
                {
                    return Results.BadRequest("Item not found with the provided ID.");
                }
                
                //Check if item of same type (eg boots) is already equiped
                var itemToEquip = user.Items.FirstOrDefault((item) => item.Id == itemId);
                var alreadyEquippedSameType = user.Items
                    .Where(item => item.Type == itemToEquip.Type && user.EquipedItemsId.Contains(item.Id))
                    .ToList();
               
                //If item of same type eg boots is already equiped, simply unequip it
                if (alreadyEquippedSameType.Count > 0)
                {
                    user.EquipedItemsId.Remove(alreadyEquippedSameType[0].Id);
                }

                user.EquipedItemsId.Add(itemId);
                await context.SaveChangesAsync(); 

                return Results.Ok($"Item equiped - id {itemId}");
            })
            .WithName("EquipItem")
            .WithOpenApi();

        return app;
    }
}