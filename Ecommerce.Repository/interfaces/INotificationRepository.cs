using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface INotificationRepository
{
    /// <summary>
    /// Method to update a range of user notification mappings in the database.
    /// </summary>
    /// <param name="userNotificationMappings"></param>
    /// <exception cref="Exception"></exception>
    void UpdateNotificationRange(List<UserNotificationMapping> userNotificationMappings);

    /// <summary>
    /// Method to get the user notification mapping for a specific user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    List<UserNotificationMapping> GetUserNotificationMapping(int userId);

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

    /// <summary>
    /// Method to add a range of user notification mappings to the database.
    /// </summary>
    /// <param name="userNotificationMappings"></param>
    /// <exception cref="Exception"></exception>
    void AddUserNotificationMappingRange(List<UserNotificationMapping> userNotificationMappings);

    /// <summary>
    /// Method to add a notification to the database.
    /// </summary>
    /// <param name="notification"></param>
    /// <exception cref="Exception"></exception>
    void AddNotification(Notification notification);

}
