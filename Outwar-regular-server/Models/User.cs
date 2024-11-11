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
    
    public string Level { get; set; }
    public int Experience { get; set; }
    public int RagePerHour { get; set; }
    public int Rage { get; set; }
    
    // Represent some kind of coordinates in world
    public int[] Location { get; set; } = new int[2];

    // Navigation property to related items
    public ICollection<Item> Items { get; set; }
    
    // Navigation property to related quests
    public ICollection<Quest> Quests { get; set; }
}