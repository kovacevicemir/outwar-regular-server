using Outwar_regular_server.Data;
using Microsoft.EntityFrameworkCore;

namespace Outwar_regular_server.Services
{
    public interface IQuestService
    {
        Task<IResult> AddQuestProgress(string username, int monsterId);
    }
    public class QuestService : IQuestService
    {
        private readonly AppDbContext context;

        public QuestService(AppDbContext dbContext)
        {
            context = dbContext;
        }

        public async Task<IResult> AddQuestProgress(string username, int monsterId)
        {
            var user = await context.Users
                    .Include(u => u.Quests) // Eagerly load the user's Quests collection
                    .FirstOrDefaultAsync(u => u.Name == username);
            if (user == null)
            {
                return Results.NotFound($"User {username} not found.");
            }

            //Find all quests with this monsterIds and that are not finished
            var questsToUpdate = user.Quests.Where(quest => quest.MonsterIds.Contains(monsterId) && quest.Status == 0).ToList();

            //Loop through each quest and update progress
            foreach (var quest in questsToUpdate)
            {
                // Convert MonsterIds (ICollection<int>) to List<int> so we can use indexof
                var monsterIdsList = quest.MonsterIds.ToList();
                // Find the index of the monsterId in the list
                var progressIndex = monsterIdsList.IndexOf(monsterId);
                if (progressIndex != -1)
                {
                    var progressList = quest.Progress.ToList(); //convert to list so we can use list[index]
                    progressList[progressIndex] = progressList[progressIndex] + 1;
                    quest.Progress = progressList;
                }

                //Check if quest is finished
                var isQuestDone = AreProgressValid(quest.Requirements, quest.Progress);
                if (isQuestDone)
                {
                    quest.Status = 1;
                }
            }

            await context.SaveChangesAsync();

            return Results.Ok($"Quest progress updated for user:{user.Name}");
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
}
