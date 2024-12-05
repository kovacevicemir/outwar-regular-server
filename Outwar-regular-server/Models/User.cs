using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Outwar_regular_server.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int RagePerHour { get; set; } = 10;
    public int Rage { get; set; } = 2000;

    public int SkillPoints { get; set; } = 0;
    
    // Represent some kind of coordinates in world
    public int[] Location { get; set; } = new int[2] { 4, 0 };

    // Navigation property to related items
    public ICollection<Item> Items { get; set; }
    
    // Represent some kind of coordinates in world
    public ICollection<int> EquipedItemsId { get; set; } = [];
    
    // Navigation property to related quests
    public ICollection<Quest> Quests { get; set; }
    
    public string? CrewName { get; set; }
    public int Points { get; set; } = 0;
    
    //Level |   0    |   0    |
    //Index |   0    |   1    |  
    //Name  | Empower|Stealth |
    public int[] Skills { get; set; } = new int[2];
}