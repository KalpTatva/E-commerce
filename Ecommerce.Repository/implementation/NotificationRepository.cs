using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    private readonly EcommerceContext _context;
    public NotificationRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }


    /// <summary>
    /// Method to retrieve notifications for a user based on their user ID.
    /// This method fetches notifications that are unread for the specified user.
    /// </summary>
    /// <param name="userId">ID of the user for whom notifications are to be retrieved</param>
    /// <returns>List of Notification containing user's notifications</returns>
    public List<Notification>? GetNotificationsByUserId(int userId)
    {
        try
        {
            List<Notification>? notifications = _context.UserNotificationMappings
                .Where(unm => unm.UserId == userId && unm.ReadAll == false)
                .Join(_context.Notifications,
                    unm => unm.NotificationId,
                    n => n.NotificationId,
                    (unm, n) => n)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return notifications;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// Method to get the count of notifications for a user based on their user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>int</returns>
    /// <exception cref="Exception"></exception>
    public int GetNotificationCount(int userId)
    {
        try
        {
            int count = _context.UserNotificationMappings
                .Count(unm => unm.UserId == userId && unm.ReadAll == false);
            return count;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
