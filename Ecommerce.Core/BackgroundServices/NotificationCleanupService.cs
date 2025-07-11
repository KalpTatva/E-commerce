using Ecommerce.Repository.Models;
using Org.BouncyCastle.Utilities;

namespace Ecommerce.Core.BackgroundServices;

public class NotificationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromDays(1); 

    public NotificationCleanupService(
        IServiceProvider serviceProvider,
        ILogger<NotificationCleanupService> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification cleanup service triggered");
        while(!stoppingToken.IsCancellationRequested) // will continue to run until cancellation 
        {
            try
            {
                _logger.LogInformation("Running notification cleanup at {time}", DateTime.Now);
                await DeleteReadNotifications(stoppingToken);
                _logger.LogInformation("cleanup task complete, waiting for next call");
                await Task.Delay(_interval, stoppingToken);     // waiting for the specific interval before next call
            }
            catch(Exception e)
            {
                _logger.LogInformation(e, "An error occured during cleaning notification");
            }
        }
        _logger.LogInformation("cleanup service stopped");
    }

    private async Task DeleteReadNotifications(CancellationToken stoppingToken)
    {
        // creating scope for DI 
        using (IServiceScope? scope = _serviceProvider.CreateScope())
        {
            EcommerceContext? context = scope.ServiceProvider.GetRequiredService<EcommerceContext>();

            List<UserNotificationMapping> readNotification = context.UserNotificationMappings.Where(static x => x.IsRead == true && x.EditedAt < DateTime.Now.Date.AddDays(-3)).ToList();

            if (readNotification.Any()) 
            {
                _logger.LogInformation("3 days old messages (notifications) count : {count}", readNotification.Count);
                context.UserNotificationMappings.RemoveRange(readNotification);
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Successfully deleted notifications");
            }
            else 
            {
                _logger.LogInformation("no new read notification found");
            }
        }   
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("cleaning service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
