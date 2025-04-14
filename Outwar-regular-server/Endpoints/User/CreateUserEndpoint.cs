using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints;

public static class CreateUserEndpoint
{
    public static IEndpointRouteBuilder MapCreateUser(this IEndpointRouteBuilder app)
    {
        app.MapPost("/create-user", async (AppDbContext context, string username) =>
            {
                // Check if the user already exists
                var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Name == username);
                if (existingUser != null)
                {
                    return Results.BadRequest("Username already exists.");
                }

                var user = new Models.User() { Name = username, EquipedItemsId = []};
                context.Users.Add(user);
                await context.SaveChangesAsync(); 
        
                return Results.Ok($"Username added: {username}");
            })
            .WithName("MockAddUser")
            .WithOpenApi();

        return app;
    }
}