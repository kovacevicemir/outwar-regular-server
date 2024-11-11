using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Outwar_regular_server.Models;

//Everything else can be calculated and represented with this data.
//Eg we can display item info using frontend lookup for viewing items only...
//Eg we can display quest progress by using hardcoded json file to display 5/10 monsters killed
//Also we dont have to relate any quest to any NPC, we can just call show quest in map at x,y location
//at some NPC eg Tony. 1. Talk 2. Show (front end hardcoded) 3. Start 4. Show progress 5. Finish

public class Quest
{   
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }  // Unique identifier for the quest
    public string Name { get; set; }  // Name of the quest
    public int Status { get; set; }  // Quest status (0Started, 1Completed, )
    
    // List of quest requirements
    // for example [10,10,10] it means it needs to kill or collect 10 monsters/items
    public ICollection<int> Requirements { get; set; }  
    
    // for example [10,10,10] same as above just keep track on progress
    public ICollection<int> Progress { get; set; }  
    
    public int Exp { get; set; }  // Experience points as reward
    public ICollection<string> ItemRewardNames { get; set; }  // Items awarded as a reward
    
    // Navigation property to related User
    public User User { get; set; }
    
}