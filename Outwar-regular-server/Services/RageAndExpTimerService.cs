using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Services
{
    public class RageAndExpTimerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public RageAndExpTimerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initial delay of 5 minutes before the loop starts
            // This prevents backend crash, because in order to run this tables needs to exists in db
            // Since docker compose first created db container than backend but it takes some time for
            // migrations to run this fails before that happens - so add delay (its not too important anyway)
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var users = context.Users.Include(u => u.Items).ToList();

                    foreach (var user in users)
                    {
                        var equippedItems = user.Items
                            .Where(item => user.EquipedItemsId.Contains(item.Id))
                            .ToList();

                        var expPerHour = equippedItems.Sum(item => item.Stats[4]);
                        var ragePerHour = equippedItems.Sum(item => item.Stats[3]) + 10;
                        var maxRagePerHour = equippedItems.Sum(item => item.Stats[2]) + 2000;

                        user.Experience += expPerHour;

                        var newRage = user.Rage + ragePerHour;
                        user.Rage = Math.Min(newRage, maxRagePerHour);
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
            }
        }
    }
}
