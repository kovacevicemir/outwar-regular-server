using System.Text.Json;
using StackExchange.Redis;

namespace Outwar_regular_server.Utilities;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class Player
{
    public string Name { get; set; }
    public int Attack { get; set; } = 1000;
}

public class BuffManager
{
    private readonly IDatabase _redis;

    public BuffManager(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    // Buff details stored in Redis
    public class BuffData
    {
        public int BonusValue { get; set; }
        public DateTime ExpirationTime { get; set; }
    }

    // Cast a buff
    public void CastBuff(string playerName, string skillName, int bonusValue, TimeSpan duration)
    {
        var key = $"skills-{playerName}-{skillName}";
        var expirationTime = DateTime.UtcNow.Add(duration);

        // Serialize buff data to JSON
        var buffData = new BuffData
        {
            BonusValue = bonusValue,
            ExpirationTime = expirationTime
        };
        var value = JsonSerializer.Serialize(buffData);

        _redis.StringSet(key, value, duration); // Set value with expiration
        Console.WriteLine($"Cast {skillName} on {playerName} with bonus {bonusValue}. Expires at {expirationTime} UTC.");
    }

    // Get all active buffs for a player
    public Dictionary<string, (int BonusValue, TimeSpan TimeRemaining)> GetActiveBuffs(string playerName)
    {
        var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: $"skills-{playerName}-*").ToArray();

        var buffs = new Dictionary<string, (int BonusValue, TimeSpan TimeRemaining)>();
        foreach (var key in keys)
        {
            var skillName = key.ToString().Split('-').Last();
            var value = _redis.StringGet(key);
            if (value.HasValue)
            {
                // Deserialize the buff data
                var buffData = JsonSerializer.Deserialize<BuffData>(value);
                if (buffData != null)
                {
                    var timeRemaining = buffData.ExpirationTime - DateTime.UtcNow;
                    buffs[skillName] = (buffData.BonusValue, timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero);
                }
            }
        }

        return buffs;
    }
}