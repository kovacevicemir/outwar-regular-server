using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.Items;

public static class DeleteItemFromUserEndpoint
{
    public static IEndpointRouteBuilder MapDeleteITemFromUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/delete-item-from-user-by-item-id", async (AppDbContext context, string username, int itemId) =>
            {
                var user = await context.Users
                    .Include(u => u.Items) // Eagerly load the user's Items collection
                    .FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }
    
                var itemToRemove = user.Items.FirstOrDefault(i => i.Id == itemId);
                if (itemToRemove == null)
                {
                    return Results.NotFound($"Item with ID {itemId} not found for user {username}.");
                }
        
                // Remove the item from the user's Items collection
                user.Items.Remove(itemToRemove);

                // Save changes to the database
                await context.SaveChangesAsync();

                return Results.Ok($"Item with ID {itemId} - {itemToRemove.Name} successfully removed from user {username}.");
            })
            .WithName("DeleteItemFromUser")
            .WithOpenApi();
        
        return app;
    }
}