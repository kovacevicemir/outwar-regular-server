using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using Outwar_regular_server.Services;

namespace Outwar_regular_server.Endpoints.MonsterNameSpace;

public static class AttackMonsterByNameEndpoint
{
    private static List<Monster>? monsters;
    private static bool monstersLoaded = false; // Flag to indicate if monsters have been loaded
    private static readonly Random _random = new Random();

    public static IEndpointRouteBuilder MapAttackMonsterByName(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapPost("/attack-monster-by-name", async (AppDbContext context, IUserService userService, IItemService itemService, IQuestService questService, ISkillService skillService, string monsterName, string username) =>
            {
                if (!monstersLoaded)
                {
                    var jsonFilePath = Path.Combine("Data", "Monsters.json");
                    try
                    {
                        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                        monsters = await JsonSerializer.DeserializeAsync<List<Monster>>(stream) ?? new List<Monster>();
                        monstersLoaded = true; // Set the flag to true after loading
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest($"Error reading the Monsters.json file: {ex.Message}");
                    }
                }

                if (monsters == null || !monsters.Any())
                {
                    return Results.NotFound("Monsters not found or the file is empty.");
                }

                var monster =
                    monsters.FirstOrDefault(m => m.Name.Equals(monsterName, StringComparison.OrdinalIgnoreCase));
                if (monster == null)
                {
                    return Results.NotFound("Monster not found. Please check the monster name.");
                }

                var message = $"{monster.Name} is killed! Exp: {monster.Exp}";

                var user = await context.Users
                    .Include(u => u.Items)
                    .FirstOrDefaultAsync(u => u.Name == username);
                if (user == null)
                {
                    return Results.NotFound($"User {username} not found.");
                }

                //Check rage
                if (user.Rage <= monster.Rage)
                {
                    return Results.BadRequest("You dont have enough rage to attack this monster!");
                }
                else
                {
                    user.Rage -= monster.Rage;
                    await context.SaveChangesAsync();
                }

                //Generate Fight
                var fightOutcome = GenerateFightOutcome(user, monster, skillService);

                if(fightOutcome.Win == false)
                {
                    return Results.Ok(fightOutcome);
                }


                var response = await userService.IncreaseExpAsync(user.Name, monster.Exp);

                if (response is IStatusCodeHttpResult status && status.StatusCode != 200)
                {
                    return Results.BadRequest("Error requesting increase-exp inside AttackMonsterByName endpoint.");
                }

                var dropBags = DetermineDrops(monster);
                if (dropBags.Count > 0)
                {
                    var tasks = dropBags.Select(item =>
                    {
                        return itemService.AddItemToUser(username, item);
                    });

                    // Await all the tasks to be completed
                    await Task.WhenAll(tasks);

                    message += $" Drops: {string.Join(", ", dropBags)}";
                }

                await questService.AddQuestProgress(username, monster.Id);

                fightOutcome.Message = message;

                return Results.Ok(fightOutcome);
            })
            .WithName("AttackMonsterByName")
            .WithOpenApi();

        return app;
    }

    public static List<string> DetermineDrops(Monster monster)
    {
        var dropBag = new List<string>();

        // Ensure we have valid data for Drops and DropsChance
        if (monster.Drops == null || monster.DropsChance == null || monster.Drops.Count != monster.DropsChance.Count)
        {
            Console.WriteLine("Invalid data: Drops and DropsChance must be of the same length.");
            return dropBag;
        }

        // Loop over each drop and corresponding chance
        for (int i = 0; i < monster.Drops.Count; i++)
        {
            var dropChance = monster.DropsChance[i];
            var dropItem = monster.Drops[i];

            // Generate a random number between 1 and 100
            var randomNumber = new Random().Next(1, 101); // Random number between 1 and 100

            // If the random number is less than or equal to the dropChance, the item drops
            if (randomNumber <= dropChance)
            {
                dropBag.Add(dropItem); // Add the drop item to the dropBag
            }
        }

        return dropBag;
    }

    public static FightOutcome GenerateFightOutcome(User user, Monster monsterInput, ISkillService skillService)
    {
        var monster = monsterInput.MonsterDeepClone();

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

        var fightOutcome = new FightOutcome();
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

            // [Monster attack]

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
                fightOutcome.MonsterAttacks.Add(monsterAttack );
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

    public class FightOutcome
    {
        public string Message { get; set; } = string.Empty;
        public List<int> PlayerAttacks { get; set; } = new List<int>();
        public List<int> MonsterAttacks { get; set; } = new List<int>();
        public List<int> PlayerHpLeft { get; set; } = new List<int>();
        public List<int> MonsterHpLeft { get; set; } = new List<int>();
        public bool Win { get; set; } = false;
    }
}