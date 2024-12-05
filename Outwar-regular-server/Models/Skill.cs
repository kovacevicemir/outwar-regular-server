namespace Outwar_regular_server.Models;

public class Skill
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int[] LevelValues { get; set; } = new int[10];
    //Can be Attack, HealthPoints, Block etc...
    public string Attribute { get; set; }
}
