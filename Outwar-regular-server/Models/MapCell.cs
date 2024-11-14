namespace Outwar_regular_server.Models;

public class MapCell
{
    public int id { get; set; }
    public string Name { get; set; }
    public ICollection<Monster> Monsters { get; set; }
    public ICollection<Npc> Npcs { get; set; }
}