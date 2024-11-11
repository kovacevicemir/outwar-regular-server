using Outwar_regular_server.Models;
using Microsoft.EntityFrameworkCore;

namespace Outwar_regular_server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseIdentityColumns(); // Ensures use of identity columns for auto-incrementing IDs
        base.OnModelCreating(builder);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Quest> Quests { get; set; }
}