using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.User;

public static class ChangeUserLocationEndpoint
{
    public static IEndpointRouteBuilder MapChangePlayerLocation(this IEndpointRouteBuilder app)
    {
        app.MapPost("/change-user-location", async (AppDbContext context, string username, string direction) =>
            {
                
                int[] PlayerLocation = { 0, 0 };
    
                // Fetch the user from the database
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Name == username);

                if (user == null)
                {
                    return Results.Problem("User not found when fetching user by username in change player location");
                }

                //Move logic starts here
                int x = user.Location[0];
                int y = user.Location[1];

                // Calculate new position based on direction
                int newX = x;
                int newY = y;

                switch (direction.ToLower())
                {
                    case "up":
                        newX = x - 1;
                        break;
                    case "down":
                        newX = x + 1;
                        break;
                    case "left":
                        newY = y - 1;
                        break;
                    case "right":
                        newY = y + 1;
                        break;
                    default:
                        return Results.BadRequest("Invalid direction.");
                }

                // Check if new position is within bounds and is not 0
                if (newX >= 0 && newX < World.gameMap.GetLength(0) &&
                    newY >= 0 && newY < World.gameMap.GetLength(1) &&
                    World.gameMap[newX, newY] != 0)
                {
                    // Move player to new position
                    PlayerLocation[0] = newX;
                    PlayerLocation[1] = newY;
                    Console.WriteLine($"Player moved {direction} to ({newX}, {newY}).");
                }
                else
                {
                    return Results.BadRequest("Move blocked or out of bounds.");
                }

                user.Location[0] = PlayerLocation[0];
                user.Location[1] = PlayerLocation[1];
                context.Users.Update(user);
                
                await context.SaveChangesAsync();

                return Results.Ok(user.Location);
            })
            .WithName("ChangeUserLocation")
            .WithOpenApi();

        return app;
    }
}