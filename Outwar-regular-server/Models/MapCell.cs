using System.Text.Json.Serialization;

namespace Outwar_regular_server.Models;

public class MapCell
{
    public int id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("monsters")]
    public List<string> Monsters { get; set; } = new();

    [JsonPropertyName("npcs")]
    public List<string> Npcs { get; set; } = new();
}