using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.Items;

public static class LockItemToUserEndpoint
{
    public static IEndpointRouteBuilder MapLockItemToUserEndpoint(this IEndpointRouteBuilder app)
    {
        // If item is locked unlock it
        // If item is un-locked lock it
        app.MapPost("/lock-item-toggle", async (AppDbContext context, int itemId) =>
            {

                var item = await context.Items.FirstOrDefaultAsync(x => x.Id == itemId);
                if (item == null)
                {
                    return Results.NotFound($"Locking item... Item with id:{itemId} not found.");
                }

                var message = "";

                if (item.Locked)
                {
                    item.Locked = false;
                    message = $"Item {item.Name} is now un-locked!";
                }
                else
                {
                    item.Locked = true;
                    message = $"Item {item.Name} is now locked!";
                }

                await context.SaveChangesAsync();

                return Results.Ok(message);
            })
            .WithName("LockItem")
            .WithOpenApi();

        return app;
    }
}