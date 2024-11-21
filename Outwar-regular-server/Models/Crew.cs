using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Outwar_regular_server.Models;

public class Crew
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public int CrewLeaderId { get; set; } //Player id
    public ICollection<User> Members { get; set; }
    public ICollection<Item> VaultItems { get; set; }
    
    // |   0     |    1    |    2     |   3     |     4    |    5    |
    // |  Att    |   Hp    |   Exp    |  Rage   |   Crit   |  Block  |
    public int[] CrewUpgrades { get; set; } = new int[6];
}