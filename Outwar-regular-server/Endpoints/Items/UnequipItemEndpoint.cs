using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.Items;

public static class UnequipItemEndpoint
{
    public static IEndpointRouteBuilder MapUnequipItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/unequip-item", async (AppDbContext context, string username, int itemId) =>
            {
            
                var user = await context.Users
                    .Include(u => u.Items) // Eagerly load the user's Items collection
                    .FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }
                
                //Check if itemId exists in this current user - prevent injecting random ID
                if (!user.EquipedItemsId.Any(item => item == itemId))
                {
                    return Results.BadRequest("Item not found with the provided ID.");
                }
                
                user.EquipedItemsId.Remove(itemId);
                
                
                await context.SaveChangesAsync(); 

                return Results.Ok($"Item un-equiped - id {itemId}");
            })
            .WithName("UnequipItem")
            .WithOpenApi();

        return app;
    }
}