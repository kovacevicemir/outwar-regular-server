namespace Outwar_regular_server.Models;

public class God
{
    public string Name { get; set; }
    public int LevelRequirement { get; set; }
    public int Attack { get; set; }
    public int Hp { get; set; }
    public List<string> Drops { get; set; }
    public List<int> DropsChance { get; set; }

    public God GodDeepClone()
    {
        return new God
        {
            Name = this.Name,
            Attack = this.Attack,
            LevelRequirement = this.LevelRequirement,
            Hp = this.Hp,
            Drops = this.Drops != null ? new List<string>(this.Drops) : null,
            DropsChance = this.DropsChance != null ? new List<int>(this.DropsChance) : null
        };
    }
}