namespace Outwar_regular_server.Models;

public class Raid
{
    public int Id { get; set; }
    public string RaidName { get; set; }
    public List<User> RaidMembers { get; set; }

    //Idea is to drop items to whoever created raid - less work for crew vault etc.
    public User CreatedBy { get; set; }
    
    //Idea is to have multiple raids on same God, until hp is 0
    //This way single person can create and attack over and over again
    public int HpLeft { get; set; }
}