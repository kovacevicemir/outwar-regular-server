using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Outwar_regular_server.Models;

public class Item
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; } 
    
    [Required]
    public string Name { get; set; }
    
    //   0   | 1 |   2   |  3 | 4 |   5   |    6   |  7  
    // attack|hp |maxRage|rage|exp|rampage|critical|block
    public int[] Stats { get; set; } = new int[8];

    // attack | deffense
    //    0   |    1
    public int[] SetBonus { get; set; } = new int[2];
    
    public int UpgradeLevel { get; set; } = 0;

    // Navigation property to related User
    public User User { get; set; }
}