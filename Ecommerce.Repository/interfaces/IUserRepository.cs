using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface IUserRepository
{
    /// <summary>
    /// function for getting perticular user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    User? GetUserByEmail(string email);

    /// <summary>
    /// method for getting perticular user by userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    User? GetUserById(int userId);

    /// <summary>
    /// updates user details
    /// </summary>
    /// <param name="user">User</param>
    /// <exception cref="Exception"></exception>
    void UpdateUser(User user);


    /// <summary>
    /// updates password reset request
    /// </summary>
    /// <param name="passwordResetRequest">PasswordResetRequest</param>
    /// <exception cref="Exception"></exception>
    void UpdatePasswordResetRequest(PasswordResetRequest passwordResetRequest);

    /// <summary>
    /// function for updating session (when session is expire after 30 days, need to soft delete it in db)
    /// </summary>
    /// <param name="session"></param>
    /// <exception cref="Exception"></exception>
    void UpdateSession(Session session);


    /// <summary>
    /// function for getting session details by session id
    /// </summary>
    /// <param name="sessionId"></param>
    Session? GetSessionDetails(string sessionId);

    /// <summary>
    /// function for getting user by id
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="userId"></param>
    /// <param name="expiresAt"></param>
    void CreateSession(string sessionId, int userId, DateTime expiresAt, string jwtToken);

    /// <summary>
    /// db call for adding password reset request in db
    /// </summary>
    /// <param name="passwordResetRequest"></param>
    /// <exception cref="Exception"></exception>
    void AddPasswordResetRequest(PasswordResetRequest passwordResetRequest);

    /// <summary>
    /// getting password reset request details basaed on token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    PasswordResetRequest? GetPasswordResetRequestByToken(string token);

    List<Country>? GetCountries();
    List<State>? GetStates(int countryId);
    List<City>? GetCities(int stateId);
    
}
