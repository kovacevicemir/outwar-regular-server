using Outwar_regular_server.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Outwar_regular_server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseIdentityColumns(); // Ensures use of identity columns for auto-incrementing IDs

        // Adding this to delete item from Items table if: user.Items.Remove(itemToRemove); -- THIS DOES NOT WORK TODO
        builder.Entity<User>()
        .HasMany(u => u.Items)
        .WithOne()
        .OnDelete(DeleteBehavior.Cascade);  // Enable Cascade Delete

        base.OnModelCreating(builder);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Quest> Quests { get; set; }
    public DbSet<Crew> Crews { get; set; }
}