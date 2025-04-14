using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints;

public static class DeleteUserByUsernameEndpoint
{
    public static IEndpointRouteBuilder MapDeleteUserByUsername(this IEndpointRouteBuilder app)
    {
        app.MapPost("/delete-user", async (AppDbContext context, string username) =>
            {
                var user = context.Users.FirstOrDefault(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound();
                }
        
                context.Users.Remove(user);
                await context.SaveChangesAsync(); 
        
                return Results.Ok($"Username deleted: {username}");
            })
            .WithName("DeleteUser")
            .WithOpenApi();

        return app;
    }
}