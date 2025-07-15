using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    
    /// <summary>
    /// Method to retrieve notifications for a user based on their user ID.
    /// This method fetches notifications that are unread for the specified user.
    /// </summary>
    /// <param name="userId">ID of the user for whom notifications are to be retrieved</param>
    /// <returns>List of Notification containing user's notifications</returns>
    List<Notification>? GetNotificationsByUserId(int userId);


    /// <summary>
    /// Method to get the count of notifications for a user based on their user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    int GetNotificationCount(int userId);

}
