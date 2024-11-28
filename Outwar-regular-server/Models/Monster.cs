namespace Outwar_regular_server.Models;

public class Monster
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Attack { get; set; }
    public int Hp { get; set; }
    public int Exp { get; set; }
    public int Rage { get; set; }
    public List<string> Drops { get; set; }
    public List<int> DropsChance { get; set; }
}