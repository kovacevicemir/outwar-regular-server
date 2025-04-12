using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;

namespace Outwar_regular_server.Endpoints.Wiki;

public static class GetDropListEndpoint
{
    private static List<Monster>? monsters;
    private static bool monstersLoaded = false;
    private static List<Item>? items;
    private static bool itemsLoaded = false;
    private static List<MapCell>? worldDefinitions;
    private static bool worldDefinitionsLoaded = false;
    public static IEndpointRouteBuilder MapGetDropListEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/get-droplist", async (AppDbContext context) =>
            {
                if(!itemsLoaded || !monstersLoaded)
                {
                    try
                    {
                        var jsonFilePath = Path.Combine("Data", "Items.json");
                        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
                        items = await JsonSerializer.DeserializeAsync<List<Item>>(stream) ?? new List<Item>();
                        itemsLoaded = true;

                        var jsonFilePathMonsters = Path.Combine("Data", "Monsters.json");
                        using var stream2 = new FileStream(jsonFilePathMonsters, FileMode.Open, FileAccess.Read);
                        monsters = await JsonSerializer.DeserializeAsync<List<Monster>>(stream2) ?? new List<Monster>();
                        monstersLoaded = true;

                        var jsonFilePathWD = Path.Combine("Data", "WorldDefinitions.json");
                        using var stream3 = new FileStream(jsonFilePathWD, FileMode.Open, FileAccess.Read);
                        worldDefinitions = await JsonSerializer.DeserializeAsync<List<MapCell>>(stream3) ?? new List<MapCell>();
                        worldDefinitionsLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest($"Could not read Monsters.json or Items.Json - something went wrong! -Error reading the Items.json file: {ex.Message}");
                    }
                }

                var itemToMonstersMap = new Dictionary<string, MonstersAndLocations>();

                // Build monster -> world(s) map first
                var monsterToWorlds = new Dictionary<string, List<string>>();
                foreach (var world in worldDefinitions)
                {
                    foreach (var monsterName in world.Monsters)
                    {
                        if (!monsterToWorlds.ContainsKey(monsterName))
                            monsterToWorlds[monsterName] = new List<string>();

                        monsterToWorlds[monsterName].Add(world.Name);
                    }
                }

                foreach (var monster in monsters)
                {
                    if (monster.Drops == null) continue;

                    foreach (var drop in monster.Drops)
                    {
                        if (!itemToMonstersMap.ContainsKey(drop))
                        {
                            itemToMonstersMap[drop] = new MonstersAndLocations();
                        }

                        itemToMonstersMap[drop].MonsterNames.Add(monster.Name);

                        var locations = monsterToWorlds.TryGetValue(monster.Name, out var worldList)
                            ? worldList
                            : new List<string> { "Unknown" };

                        foreach (var loc in locations)
                        {
                            itemToMonstersMap[drop].MonsterLocations.Add(loc);
                        }
                    }
                }


                return Results.Ok(itemToMonstersMap);
            })
            .WithName("GetDropList")
            .WithOpenApi();
        return app;
    }

    // use HashSet to avoid duplicate entries.
    public class MonstersAndLocations
    {
        public HashSet<string> MonsterNames { get; set; } = new();
        public HashSet<string> MonsterLocations { get; set; } = new();
    }
}