using System.Text.Json;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.Items;

public static class AddItemToUserEndpoint
{
    public static IEndpointRouteBuilder MapAddItemToUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/add-item-to-user", async (AppDbContext context, string username, string itemName) =>
            {
                var jsonFilePath = @"Data\Items.json";
                List<Item> items;
        
                try
                {
                    using (var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read))
                    {
                        items = await JsonSerializer.DeserializeAsync<List<Item>>(stream);
                    }
                }
                catch (Exception ex)
                {
                    return Results.BadRequest($"Error reading the Items.json file: {ex.Message}");
                }
        
                if (items == null || !items.Any())
                {
                    return Results.NotFound("Items not found or the file is empty.");
                }
        
                var findItem = items.FirstOrDefault(i => i.Name == itemName);
                if (findItem == null)
                {
                    return Results.NotFound($"Item {itemName} not found in the Items.json.");
                }
        
                var user = context.Users.FirstOrDefault(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }
        
                // Ensure the Items collection is initialized.
                user.Items ??= new List<Item>(); // This is important if the collection is null.
        
                var newItem = new Item
                {
                    Name = itemName,
                    SetBonus = findItem.SetBonus,
                    Stats = findItem.Stats,
                    UpgradeLevel = findItem.UpgradeLevel,
                    User = user,
                };
        
                // Add the item to the user's collection
                user.Items.Add(newItem);
        
                // Save changes to the database (you need to make sure this context is saved)
                await context.SaveChangesAsync();
        
                return Results.Ok($"Item {findItem.Name} added to: {username}");
            })
            .WithName("AddItemToUser")
            .WithOpenApi();

        return app;
    }
}