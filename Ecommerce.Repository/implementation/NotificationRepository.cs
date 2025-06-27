using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class NotificationRepository : INotificationRepository
{
    private readonly EcommerceContext _context;
    public NotificationRepository(EcommerceContext context)
    {
        _context = context;
    }


    /// <summary>
    /// Method to update a range of user notification mappings in the database.
    /// </summary>
    /// <param name="userNotificationMappings"></param>
    /// <exception cref="Exception"></exception>
    public void UpdateNotificationRange(List<UserNotificationMapping> userNotificationMappings)
    {
        try
        {
            _context.UserNotificationMappings.UpdateRange(userNotificationMappings);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Method to get the user notification mapping for a specific user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>List<UserNotificationMapping></returns>
    /// <exception cref="Exception"></exception>
    public List<UserNotificationMapping> GetUserNotificationMapping(int userId)
    {
        try
        {
            List<UserNotificationMapping> userNotificationMappings = _context.UserNotificationMappings
                .Where(unm => unm.UserId == userId && unm.ReadAll == false)
                .ToList();

            return userNotificationMappings;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
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

    /// <summary>
    /// Method to add a range of user notification mappings to the database.
    /// </summary>
    /// <param name="userNotificationMappings"></param>
    /// <exception cref="Exception"></exception>
    public void AddUserNotificationMappingRange(List<UserNotificationMapping> userNotificationMappings)
    {
        try
        {
            _context.UserNotificationMappings.AddRange(userNotificationMappings);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Method to add a notification to the database.
    /// </summary>
    /// <param name="notification"></param>
    /// <exception cref="Exception"></exception>
    public void AddNotification(Notification notification)
    {
        try
        {
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
