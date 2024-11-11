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
    
    // Represent some kind of coordinates in world
    public int[] Location { get; set; } = new int[2];

    // Navigation property to related items
    public ICollection<Item> Items { get; set; }
    
    // Represent some kind of coordinates in world
    public ICollection<int> EquipedItemsId { get; set; }
    
    // Navigation property to related quests
    public ICollection<Quest> Quests { get; set; }
}