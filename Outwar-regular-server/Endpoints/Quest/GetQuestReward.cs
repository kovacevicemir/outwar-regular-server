using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;

namespace Outwar_regular_server.Endpoints.Items;

public static class GetQuestRewardEndpoint
{
    public static IEndpointRouteBuilder MapGetQuestRewardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/get-quest-reward", async (AppDbContext context, string username, string questName) =>
            {
                var user = await context.Users
                    .Include(u => u.Quests) // Eagerly load the user's Items collection
                    .FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }

               //Find all quests with this monsterIds and that are not finished
               var quest = user.Quests.FirstOrDefault(q => q.Name == questName);
               if (quest == null)
               {
                   return Results.NotFound($"quest {questName} not found.");
               }
               
               //Check if already got reward
               if (quest.GotReward == 1)
               {
                   return Results.BadRequest("This quest is already completed & reward is claimed");
               }

               var isQuestDone = AreProgressValid(quest.Requirements, quest.Progress);
               if (isQuestDone)
               {
                   user.Experience = user.Experience + quest.Exp;
                   using (var client = new HttpClient())
                   {
                       foreach (var questItemRewardName in quest.ItemRewardNames)
                       {
                           var addItemToUserUrl =
                               $"https://localhost:44338/add-item-to-user?username={user.Name}&itemName={questItemRewardName}";
                           await client.PostAsync(addItemToUserUrl, null);
                           //This is returning 500 for some reason - even when it works...
                           // if (!response.IsSuccessStatusCode)
                           // {
                           //     return Results.BadRequest("Failed to add quest reward item to user");
                           // }
                       }
                   }
               }
               
               //Update quest reward - yes
               quest.GotReward = 1;
               await context.SaveChangesAsync();
               
               return Results.Ok($"Quest reward completed for user:{user.Name}");
            })
            .WithName("GetQuestReward")
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