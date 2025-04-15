using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Utilities;
using StackExchange.Redis;

namespace Outwar_regular_server.Services
{
    public interface ISkillService
    {
        List<AllActiveSkillsItem> GetAllActiveSkills(string userName);
    }
    public class SkillService : ISkillService
    {
        private readonly AppDbContext context;
        private readonly IConnectionMultiplexer redis;

        public SkillService(AppDbContext dbContext, IConnectionMultiplexer _redis)
        {
            context = dbContext;
            redis = _redis;
        }

        public List<AllActiveSkillsItem> GetAllActiveSkills(string userName)
        {
            var buffManager = new BuffManager(redis);

            // Find all buffs
            var activeBuffs = buffManager.GetActiveBuffs(userName);

            // Filter out buffs with a duration less than 23 hours
            // var filteredBuffs = activeBuffs
            //     .Where(buff => buff.Value.TimeRemaining >= TimeSpan.FromHours(23));

            // Transform the dictionary into a list of anonymous objects
            var buffsList = activeBuffs.Select(buff => new AllActiveSkillsItem
            {
                SkillName = buff.Key,
                Duration = buff.Value.TimeRemaining.ToString("hh\\:mm\\:ss"), // Format the TimeSpan
                Bonus = buff.Value.BonusValue
            }).ToList();

            return buffsList;
        }
    }

    public class AllActiveSkillsItem{
        public string SkillName { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int Bonus { get; set; }

    }
}
