using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.Items;

public static class BuyShopItemEndpoint
{
    private static List<Item>? items;
    private static bool itemsLoaded = false;
    public static IEndpointRouteBuilder MapBuyShopItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/buy-item-from-shop", async (AppDbContext context, string username, string itemName) =>
            { 
                // Load items only once
            if (!itemsLoaded)
            {
                var jsonFilePath = @"Data\Items.json";
                try
                {
                    using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                    items = await JsonSerializer.DeserializeAsync<List<Item>>(stream) ?? new List<Item>();
                    itemsLoaded = true; // Set the flag to indicate items are loaded
                }
                catch (Exception ex)
                {
                    return Results.BadRequest($"Error reading the Items.json file: {ex.Message}");
                }
            }

            // Verify items list is loaded and not empty
            if (items == null || !items.Any())
            {
                return Results.NotFound("Items not found or the file is empty.");
            }

            // Find the item in the list
            var findItem = items.FirstOrDefault(i => i.Name == itemName);
            if (findItem == null)
            {
                return Results.NotFound($"Item {itemName} not found in Items.json.");
            }

            // Find the user in the database
            var user = await context.Users.Include(u => u.Items).FirstOrDefaultAsync(u => u.Name == username);
            if (user == null)
            {
                return Results.NotFound($"User {username} not found.");
            }
            
            //Points:
            var itemWorth = 9999999;
            if (itemName == "Dead Eye")
            {
                itemWorth = 120;
            }
            if (itemName == "Bracer of Death")
            {
                itemWorth = 400;
            }
            if (itemName == "Bracer of Life")
            {
                itemWorth = 400;
            }

            if (user.Points < itemWorth)
            {
                return Results.Ok("You do not have enough points.");
            }


            // Ensure the user's Items collection is initialized
            user.Items ??= new List<Item>();

            // Create and add the new item to the user's collection
            var newItem = new Item
            {
                Name = findItem.Name,
                SetBonus = findItem.SetBonus,
                Stats = findItem.Stats,
                UpgradeLevel = findItem.UpgradeLevel,
                Type = findItem.Type
            };

            user.Items.Add(newItem);
            user.Points -= itemWorth;

            // Save changes to the database
            await context.SaveChangesAsync();

            return Results.Ok($"Item {findItem.Name} purchased successfully!");
            })
            .WithName("BuyShopItem")
            .WithOpenApi();

        return app;
    }
}