using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.User;

public static class GetUserByUsernameEndpoint
{
    public static IEndpointRouteBuilder MapGetUserByUsername(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-user-by-username", async (AppDbContext context, string username) =>
            {
                var user = await context.Users
                    .Include(u => u.Items) // Eagerly load the user's Items collection
                    .FirstOrDefaultAsync(u => u.Name == username);
                
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }
        
                return Results.Ok(user);
            })
            .WithName("GetUserByUsername")
            .WithOpenApi();

        return app;
    }
}