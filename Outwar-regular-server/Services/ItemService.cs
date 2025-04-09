using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Outwar_regular_server.Services
{
    public interface IItemService
    {
        Task<IResult> AddItemToUser(string username, string itemName);
    }
    public class ItemService : IItemService
    {
        private readonly AppDbContext context;
        private static List<Item>? items;
        private static bool itemsLoaded = false;
        private static bool experienceListLoaded = false;

        public ItemService(AppDbContext dbContext)
        {
            context = dbContext;
        }

        public async Task<IResult> AddItemToUser(string username, string itemName)
        {
            // Load items only once
            if (!itemsLoaded)
            {
                var jsonFilePath = Path.Combine("Data", "Items.json");

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

            // Save changes to the database
            await context.SaveChangesAsync();

            return Results.Ok($"Item {findItem.Name} added to: {username} - item id: {newItem.Id}");
        }
    }
}
