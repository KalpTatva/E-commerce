using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Service.interfaces;

public interface IUserService
{
    /// <summary>
    /// service for user login 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    ResponseTokenViewModel UserLogin(LoginViewModel model);

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
    /// inorder to refresh token if its going to expire in few minutes
    /// </summary>
    /// <param name="email"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    ResponseTokenViewModel RefreshToken(string email, string role);

    /// <summary>
    /// method for generating link for reset password
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<ResponsesViewModel> ForgotPassword(EmailViewModel email);

    /// <summary>
    /// method for validate reset password link 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    ResponsesViewModel ValidateResetPasswordToken(string token);

    /// <summary>
    /// resets password 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    ResponsesViewModel ResetPassword(ForgetPasswordViewModel model);

    /// <summary>
    /// method for registering a new user
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    ResponsesViewModel RegisterUser(RegisterUserViewModel model);

    /// <summary>
    /// method for getting countrries
    /// </summary>
    /// <returns></returns>
    List<Country>? GetCountries();

    /// <summary>
    /// method for getting states based on country 
    /// </summary>
    /// <param name="countryId"></param>
    /// <returns></returns>
    List<State>? GetStates(int countryId);

    /// <summary>
    /// method for getting cities based on state id
    /// </summary>
    /// <param name="stateId"></param>
    /// <returns></returns>
    List<City>? GetCities(int stateId);

    /// <summary>
    /// method for getting user + profile data using email
    /// </summary>
    /// <param name="email"></param>
    /// <returns> returns EditRegisteredUserViewModel </returns>
    EditRegisteredUserViewModel? GetUserDetailsByEmail(string email);

    /// <summary>
    /// method for edit user and profile
    /// </summary>
    /// <param name="model"></param>
    /// <returns>ResponsesViewModel</returns>
    ResponsesViewModel EditUserDetails(EditRegisteredUserViewModel model);
}
