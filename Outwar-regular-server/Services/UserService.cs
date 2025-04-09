using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Outwar_regular_server.Services
{
    public interface IUserService
    {
        Task<IResult> IncreaseExpAsync(string username, int exp);
    }
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;
        private static List<ExperienceListItem> experienceList;
        private static bool experienceListLoaded = false;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IResult> IncreaseExpAsync(string username, int exp)
        {
            // Load experience list only once
            if (!experienceListLoaded)
            {
                var jsonFilePath = Path.Combine("Data", "experienceList.json");

                try
                {
                    using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                    experienceList = await JsonSerializer.DeserializeAsync<List<ExperienceListItem>>(stream) ?? new List<ExperienceListItem>();
                    experienceListLoaded = true; // Set flag to prevent reloading ExperienceListItem
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading the experienceList.json file: {ex.Message}");
                    return Results.BadRequest("Error loading experience list.");
                }
            }

            // Verify experience list has been loaded successfully
            if (experienceList == null || !experienceList.Any())
            {
                return Results.NotFound("Experience list not found or empty.");
            }

            // Find the user
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Name == username);
            if (user == null)
            {
                return Results.NotFound($"User {username} not found.");
            }

            // Update experience points
            user.Experience += exp;

            // Check if the user levels up
            bool isLevelUp = false;

            var nextLevel = experienceList[user.Level];
            if (nextLevel != null)
            {
                if (user.Experience >= nextLevel.Experience)
                {
                    user.Level += 1;
                    isLevelUp = true;

                    //Add skill point - only add if needed...
                    if (experienceList[user.Level].SkillPoints > nextLevel.SkillPoints)
                    {
                        user.SkillPoints += 1;
                    }
                }
            }

            await _dbContext.SaveChangesAsync();

            return Results.Ok($"Added {exp} experience to {username}. Level up: {isLevelUp}");
        }
    }
}
