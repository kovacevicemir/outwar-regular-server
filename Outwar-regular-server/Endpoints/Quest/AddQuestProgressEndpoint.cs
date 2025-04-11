using Outwar_regular_server.Data;
using Outwar_regular_server.Services;

namespace Outwar_regular_server.Endpoints.Items;

public static class AddQuestProgressEndpoint
{
    public static IEndpointRouteBuilder MapAddQuestProgressEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/add-quest-progress", async (AppDbContext context, IQuestService questService, string username, int monsterId) =>
            {
               await questService.AddQuestProgress(username, monsterId);
            })
            .WithName("AddQuestProgress")
            .WithOpenApi();

        return app;
    }
    
    //Check if quest is done basically
    public static bool AreProgressValid(ICollection<int> requirements, ICollection<int> progress)
    {
        // Check if both collections have the same length
        if (requirements.Count != progress.Count)
        {
            return false; // Collections should be the same length
        }

        // Loop through both collections and compare each element pair
        for (int i = 0; i < requirements.Count; i++)
        {
            if (progress.ElementAt(i) < requirements.ElementAt(i))
            {
                return false; // Return false if any progress is less than the requirement
            }
        }

        return true; // All elements are valid
    }
}