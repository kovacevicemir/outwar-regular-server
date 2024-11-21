namespace Outwar_regular_server.Models;

public class God
{
    public string Name { get; set; }
    public int LevelRequirement { get; set; }
    public int Attack { get; set; }
    public int Hp { get; set; }
    public List<string> Drops { get; set; }
    public List<int> DropsChance { get; set; }
}