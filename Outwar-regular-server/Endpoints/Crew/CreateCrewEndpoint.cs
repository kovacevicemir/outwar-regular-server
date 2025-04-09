using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.Items;

public static class CreateCrewEndpoint
{
    public static IEndpointRouteBuilder MapCreateCrewEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/create-crew", async (AppDbContext context, int crewLeaderId, string crewName) =>
            {
            // Find the user in the database
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == crewLeaderId);
            if (user == null)
            {
                return Results.NotFound($"Creating crew, crewLeaderId (userId) {crewLeaderId} not found.");
            }

            // Check if crew name already exists
            var crewNameExists = await context.Crews.Where(c => c.Name == crewName).FirstOrDefaultAsync(); 
            if(crewNameExists is not null)
            {
                    return Results.BadRequest($"Crew {crewName} already exists!");
            }

            var newCrew = new Crew()
            {
                Name = crewName,
                CrewLeaderId = crewLeaderId,
                Members = new List<Models.User> { user },
                VaultItems = [],
                CrewUpgrades = [0,0,0,0,0,0]
            };

            context.Crews.Add(newCrew);
            
            user.CrewName = newCrew.Name;

            // Save changes to the database
            await context.SaveChangesAsync();

            return Results.Ok($"Crew ${newCrew.Name} created! Crew leader: ${user.Name}");
            })
            .WithName("CreateCrew")
            .WithOpenApi();

        return app;
    }
}