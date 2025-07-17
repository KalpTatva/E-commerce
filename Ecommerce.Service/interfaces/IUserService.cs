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
    /// inorder to refresh token if its going to expire in few minutes
    /// </summary>
    /// <param name="email"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    ResponseTokenViewModel RefreshToken(string email, string role, string UserName);

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
    Task<ResponsesViewModel> ValidateResetPasswordToken(string token);

    /// <summary>
    /// resets password 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<ResponsesViewModel> ResetPassword(ForgetPasswordViewModel model);

    /// <summary>
    /// method for registering a new user
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<ResponsesViewModel> RegisterUser(RegisterUserViewModel model);

    /// <summary>
    /// method for getting countrries
    /// </summary>
    /// <returns></returns>
    Task<List<Country>?> GetCountries();

    /// <summary>
    /// method for getting states based on country 
    /// </summary>
    /// <param name="countryId"></param>
    /// <returns></returns>
    Task<List<State>?> GetStates(int countryId);

    /// <summary>
    /// method for getting cities based on state id
    /// </summary>
    /// <param name="stateId"></param>
    /// <returns></returns>
    Task<List<City>?> GetCities(int stateId);

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
    Task<ResponsesViewModel> EditUserDetails(EditRegisteredUserViewModel model);


    /// <summary>
    /// Method to add a contact message from the user.
    /// This method creates a new Contactu object with the provided details,
    /// adds it to the repository, and sends an email notification to the recipient.
    /// </summary>
    /// <param name="model">ContactUsViewModel containing the contact message details</param>
    Task<ResponsesViewModel> AddContactMessage(ContactUsViewModel model);


    Task<ResponsesViewModel> ThemeChange(string theme, string email); 


    Task<SellersViewModel> GetSellers();

    
    Task<ResponsesViewModel> GrantOFferService(List<GrantOfferPermission> ids);

}
