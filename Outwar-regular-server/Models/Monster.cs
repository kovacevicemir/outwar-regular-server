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

    public Monster MonsterDeepClone()
    {
        return new Monster
        {
            Id = this.Id,
            Name = this.Name,
            Attack = this.Attack,
            Hp = this.Hp,
            Exp = this.Exp,
            Rage = this.Rage,
            Drops = this.Drops != null ? new List<string>(this.Drops) : null,
            DropsChance = this.DropsChance != null ? new List<int>(this.DropsChance) : null
        };
    }
}