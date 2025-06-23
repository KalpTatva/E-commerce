using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

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

    /// <summary>
    /// function for adding user in db
    /// </summary>
    /// <param name="user"></param>
    void AddUser(User user);

    /// <summary>
    /// function for adding profile in db
    /// </summary>
    /// <param name="profile"></param>
    void AddProfile(Profile profile);

    /// <summary>
    /// function fo getting countries
    /// </summary>
    /// <returns></returns>
    List<Country>? GetCountries();

    /// <summary>
    /// function for getting states based on country id
    /// </summary>
    /// <param name="countryId"></param>
    /// <returns></returns>
    List<State>? GetStates(int countryId);

    /// <summary>
    /// function for getting cities based on state id
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
    /// method for getting profile based on profile id
    /// </summary>
    /// <param name="profileId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Profile? GetProfileById(int profileId);

    /// <summary>
    /// method for update profile
    /// </summary>
    /// <param name="profile"></param>
    /// <exception cref="Exception"></exception>
    void UpdateProfile(Profile profile);

    /// <summary>
    /// method for get user's country name
    /// </summary>
    /// <param name="countryId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    string? GetCityNameById(int CityId);

    /// <summary>
    /// method for get user's state name
    /// </summary>
    /// <param name="stateId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    string? GetStateNameById(int stateId);

    /// <summary>
    /// method for get user's city name
    /// </summary>
    /// <param name="CityId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    string? GetCountryNameById(int countryId);
    
}
