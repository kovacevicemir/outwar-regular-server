using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using Outwar_regular_server.Services;
using StackExchange.Redis;

namespace Outwar_regular_server.Endpoints.Items;

public static class AttackRaidEndpoint
{
    private static List<God>? gods;
    private static bool godsLoaded = false;
    private static readonly Random _random = new Random();
    public static IEndpointRouteBuilder MapAttackRaidEndpoint(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapPost("/attack-raid", async (AppDbContext context, IItemService itemService, ISkillService skillService, IConnectionMultiplexer redis, string crewName, string raidName) =>
            {
                
                if (!godsLoaded)
                {
                    var jsonFilePath = Path.Combine( "Data", "Gods.json");
                    try
                    {
                        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                        gods = await JsonSerializer.DeserializeAsync<List<God>>(stream) ?? new List<God>();
                        godsLoaded = true; // Set the flag to indicate items are loaded

                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest($"Error reading the Gods.json file: {ex.Message}");
                    }
                }
                
                if (gods == null || !gods.Any())
                {
                    return Results.NotFound("Gods not found or the file is empty.");
                }
                
                var god = gods.SingleOrDefault(g => g.Name == raidName);
                if (god == null)
                {
                    return Results.NotFound($"God - {raidName} not found or the file is empty.");
                }

                var db = redis.GetDatabase();
                var jsonValue = await db.StringGetAsync($"raid-{crewName}-{raidName}");
            
                if (jsonValue.IsNullOrEmpty)
                    return Results.NotFound($"Key raid-{crewName}-{raidName} not found");
            
                var deserializedRaid = JsonSerializer.Deserialize<Raid>(jsonValue);

                //Decrease rage for raid members
                var membersToRemove = new List<User>();

                foreach (var raidMember in deserializedRaid.RaidMembers)
                {
                    var raidMembFromDb = await context.Users.Where(u => u.Id == raidMember.Id).SingleOrDefaultAsync();
                    if (raidMembFromDb == null)
                    {
                        return Results.BadRequest("User does not exists! Inside AttackRaid Decreasing rage.");
                    }

                    if (raidMembFromDb.Rage < 50)
                    {
                        membersToRemove.Add(raidMember); // Mark for removal
                    }
                    else
                    {
                        raidMembFromDb.Rage -= 50;
                        await context.SaveChangesAsync();
                    }
                }

                //Check if raid members? If nobody have enough rage - its expected to raidMembers to be empty
                foreach (var member in membersToRemove)
                {
                    deserializedRaid.RaidMembers.Remove(member);
                }
                if (deserializedRaid.RaidMembers.Count == 0)
                {
                    return Results.Ok("There is no single raid member with enough rage (50) to attack!");
                }

                // Set hp left
                god.Hp = deserializedRaid.HpLeft;

                var raidOutcome = GenerateFightOutcome(deserializedRaid.CreatedBy, god ,skillService);

                deserializedRaid.HpLeft = raidOutcome.MonsterHpLeft[raidOutcome.MonsterHpLeft.Count-1]; //Get last digit in godHpLeft log

                var message = $"Attack {raidName} done! {raidOutcome.Message}";

                // Check if raid is done
                if(deserializedRaid.HpLeft <= 0)
                {
                        var dropBags = DetermineDrops(god);
                        
                        if (dropBags.Count > 0)
                        {
                            var tasks = dropBags.Select(item =>
                            {
                                return itemService.AddItemToUser(deserializedRaid.CreatedBy.Name, item);
                            });
                                
                            // Await all the tasks to be completed
                            await Task.WhenAll(tasks);
                            
                            message += $" Drops: {string.Join(", ", dropBags)}";
                        }
                        else //Reward some points to user who created raid - if no drops
                        {
                            Random rnd = new Random();
                            var pointsToAdd = rnd.Next(1, 10); 
                                
                            var user = context.Users.FirstOrDefault(u => u.Name == deserializedRaid.CreatedBy.Name);
                            if (user != null)
                            {
                                user.Points += pointsToAdd;
                                await context.SaveChangesAsync();
                                message += $" Points: {pointsToAdd}";
                            }
                        }
                    
                    var isDeleted = await db.KeyDeleteAsync($"raid-{crewName}-{raidName}");
                }
                else
                {
                    message += $" Hp left: {deserializedRaid.HpLeft}";
                    var jsonRaid = JsonSerializer.Serialize(deserializedRaid);
                    await db.StringSetAsync($"raid-{crewName}-{raidName}", jsonRaid);
                }

                return Results.Ok(message);
            })
            .WithName("AttackRaid")
            .WithOpenApi();

        return app;
    }
    
    public static List<string> DetermineDrops(God god)
    {
        var dropBag = new List<string>();

        // Ensure we have valid data for Drops and DropsChance
        if (god.Drops == null || god.DropsChance == null || god.Drops.Count != god.DropsChance.Count)
        {
            Console.WriteLine("Invalid data: Drops and DropsChance must be of the same length.");
            return dropBag;
        }

        // Loop over each drop and corresponding chance
        for (int i = 0; i < god.Drops.Count; i++)
        {
            var dropChance = god.DropsChance[i];
            var dropItem = god.Drops[i];

            // Generate a random number between 1 and 100
            var randomNumber = new Random().Next(1, 101);  // Random number between 1 and 100

            // If the random number is less than or equal to the dropChance, the item drops
            if (randomNumber <= dropChance)
            {
                dropBag.Add(dropItem);  // Add the drop item to the dropBag
            }
        }

        return dropBag;
    }

    public static RaidOutcome GenerateFightOutcome(User user, God godInput, ISkillService skillService)
    {
        var monster = godInput.GodDeepClone();

        // Helper variables
        var isFightDone = false;

        // Default stats
        var totalAttack = 10;
        var totalHp = 100;
        var totalCrit = 0;
        var totalBlock = 0;
        var totalRampage = 0;

        // Calculate total stats for user
        //   0   | 1 |   2   |  3 | 4 |   5   |    6   |  7  
        // attack|hp |maxRage|rage|exp|rampage|critical|block
        var equipedItems = user.Items
        .Where(item => user.EquipedItemsId.Contains(item.Id))
        .ToList();

        foreach (var item in equipedItems)
        {
            totalAttack += item.Stats[0] | 0;
            totalHp += item.Stats[1] | 0;
            totalCrit += item.Stats[6] | 0;
            totalBlock += item.Stats[7] | 0;
            totalRampage += item.Stats[5] | 0;
        }

        //Add skills if any casted?
        var activeSkills = skillService.GetAllActiveSkills(user.Name);
        foreach (var activeSkill in activeSkills)
        {
            switch (activeSkill.SkillName)
            {
                case "Empower":
                    totalAttack += activeSkill.Bonus;
                    break;
                case "Stealth":
                    totalHp += activeSkill.Bonus;
                    break;
                default:
                    break;
            }
        }

        var fightOutcome = new RaidOutcome();
        fightOutcome.PlayerHpLeft.Add(totalHp); //Add starting hp log
        fightOutcome.MonsterHpLeft.Add(monster.Hp); //Add starting hp log

        while (isFightDone == false)
        {
            // [Player attack]

            var critDamage = 0; //just place holder if it happens

            var isCrit = IsLuck(totalCrit);
            if (isCrit)
            {
                critDamage = IncreaseBy50Percent(totalAttack);
            }

            if (critDamage != 0)
            {
                var playerAttackDamage = critDamage + _random.Next(0, 10);
                var newMonsterHp = monster.Hp - playerAttackDamage;
                monster.Hp = newMonsterHp;
                fightOutcome.MonsterHpLeft.Add(newMonsterHp < 0 ? 0 : newMonsterHp); //check if hp is bellow 0, if so return 0 it makes more sense for ui
                fightOutcome.PlayerAttacks.Add(playerAttackDamage * -1); // Critical hit is representent by negative number
            }
            else
            {
                var playerAttackDamage = totalAttack + _random.Next(0, 10);
                var newMonsterHp = monster.Hp - playerAttackDamage;
                monster.Hp = newMonsterHp;
                fightOutcome.MonsterHpLeft.Add(newMonsterHp < 0 ? 0 : newMonsterHp); //check if hp is bellow 0, if so return 0 it makes more sense for ui
                fightOutcome.PlayerAttacks.Add(playerAttackDamage);
            }


            if (monster.Hp <= 0)
            {
                isFightDone = true;
                fightOutcome.Win = true;
            }

            // [God attack]

            var isBlock = IsLuck(totalBlock); //Check if player blocks monster attack

            if (isBlock)
            {
                fightOutcome.PlayerHpLeft.Add(totalHp); //It remains the same if attack is blocked
                fightOutcome.MonsterAttacks.Add(-1); // -1 represents blocked attack, since only player can block it its fine...
            }
            else
            {
                var monsterAttack = monster.Attack + _random.Next(0, 10);
                var newTotalHp = totalHp - monsterAttack;
                totalHp = newTotalHp;
                fightOutcome.PlayerHpLeft.Add(newTotalHp < 0 ? 0 : newTotalHp); //check if hp is bellow 0, if so return 0 it makes more sense for ui
                fightOutcome.MonsterAttacks.Add(monsterAttack);
            }

            if (totalHp <= 0)
            {
                isFightDone = true;
                fightOutcome.Win = false;
                fightOutcome.Message = "You lost the fight!"; //if this value changes, frontend needs to change as well
            }
        } 

        return fightOutcome;
    }

    public static bool IsLuck(int luckPercentage)
    {
        return _random.Next(0, 100) < luckPercentage;
    }

    public static int IncreaseBy50Percent(int value) //Increase dmg by 50% if crit...
    {
        return (int)Math.Round(value * 1.5);
    }

    public class RaidOutcome
    {
        public string Message { get; set; } = string.Empty;
        public List<int> PlayerAttacks { get; set; } = new List<int>();
        public List<int> MonsterAttacks { get; set; } = new List<int>();
        public List<int> PlayerHpLeft { get; set; } = new List<int>();
        public List<int> MonsterHpLeft { get; set; } = new List<int>();
        public bool Win { get; set; } = false;
    }
}