using Ecommerce.Core.Hub;
using Ecommerce.Repository.Models;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Asn1.Cms;

namespace Ecommerce.Core.BackgroundServices;

public class OfferExpireCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationCleanupService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public OfferExpireCleanupService(
        IServiceProvider serviceProvider,
        ILogger<NotificationCleanupService> logger,
        IHubContext<NotificationHub> hubContext
    ) 
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("offer expire cleanup service triggered");
        while(!cancellationToken.IsCancellationRequested) 
        {
            try
            {
                _logger.LogInformation("Running offer expiration");
                await OfferExpireUpdate(cancellationToken);
                await Task.Delay(_interval, cancellationToken);
                
            }
            catch(Exception e)
            {
                _logger.LogInformation(e, "An error occured during cleaning iffer expire");
            }
        }
        _logger.LogInformation("cleanup service stopped");
    }

    public async Task OfferExpireUpdate(CancellationToken cancellationToken)
    {
        using (IServiceScope? scope = _serviceProvider.CreateScope())
        {
            DateTime currentTime = DateTime.Now;
            
            EcommerceContext? context = scope.ServiceProvider.GetRequiredService<EcommerceContext>();
            
            List<Offer>? offersExpired = context.Offers.Where(x => x.EndDate < currentTime && x.EditedAt != null).ToList(); 
            
            if(offersExpired.Any())
            {
                foreach(Offer offor in offersExpired)
                {
                    offor.EditedAt = currentTime;
                }
                context.Offers.UpdateRange(offersExpired);
                
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", "offers expired");
            }
            else
            {
                _logger.LogInformation("no offer available for clean up");
            }
        }
    }


    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("cleaning service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
