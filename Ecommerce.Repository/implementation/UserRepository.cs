using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repository.implementation;

public class UserRepository : IUserRepository
{
    private readonly EcommerceContext _context;

    public UserRepository(EcommerceContext context)
    {
        _context = context;
    }


    /// <summary>
    /// method for getting perticular user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public User? GetUserByEmail(string email)
    {
        try{
            return _context.Users.Where(x => x.Email.Contains(email.Trim().ToLower())).FirstOrDefault();
        }
        catch(Exception e){
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for getting perticular user by userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public User? GetUserById(int userId)
    {
        try
        {
            return _context.Users.FirstOrDefault(x => x.UserId == userId);
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while fetching user by ID.", ex);   
        }
    }

    /// <summary>
    /// updates user details
    /// </summary>
    /// <param name="user">User</param>
    /// <exception cref="Exception"></exception>
    public void UpdateUser(User user)
    {
        try
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while updating user.", ex);   
        }
    }

    /// <summary>
    /// updates password reset request
    /// </summary>
    /// <param name="passwordResetRequest"></param>
    /// <exception cref="Exception"></exception>
    public void UpdatePasswordResetRequest(PasswordResetRequest passwordResetRequest)
    {
        try
        {
            _context.PasswordResetRequests.Update(passwordResetRequest);
            _context.SaveChanges();
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while updating password reset request.", ex);   
        }
    }


    /// <summary>
    /// db call for adding password reset request in db
    /// </summary>
    /// <param name="passwordResetRequest"></param>
    /// <exception cref="Exception"></exception>
    public void AddPasswordResetRequest(PasswordResetRequest passwordResetRequest)
    {
        try{
            _context.PasswordResetRequests.Add(passwordResetRequest);
            _context.SaveChanges();
        }catch(Exception e){
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// getting password reset request details basaed on token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public PasswordResetRequest? GetPasswordResetRequestByToken(string token)
    {
        try
        {
            return _context.PasswordResetRequests.FirstOrDefault(x => x.Guidtoken == token);
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while fetching password reset request by token.", ex);   
        }
    }


    /// <summary>
    /// method for registering user
    /// </summary>
    /// <param name="user"></param>
    /// <exception cref="Exception"></exception>
    public void AddUser(User user)
    {
        try
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while registering user.", ex);
        }
    }

    /// <summary>
    /// method for registering profile
    /// </summary>
    /// <param name="profile"></param>
    /// <exception cref="Exception"></exception>
    public void AddProfile(Profile profile)
    {
        try
        {    
            _context.Profiles.Add(profile);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while registering profile.", ex);
        }
    }

    /// <summary>
    /// method for getting countries
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<Country>? GetCountries()
    {
        try
        {
            return _context.Countries.ToList() ?? new List<Country>();
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while fetching Country.", ex);   
        }
    }

    /// <summary>
    /// method for getting states by country id
    /// </summary>
    /// <param name="countryId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<State>? GetStates(int countryId)
    {
        try
        {
            return _context.States.Where(x => x.CountryId == countryId).ToList() ?? new List<State>();
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while fetching state.", ex);   
        }
    }

    /// <summary>
    /// method for getting cities by state id
    /// </summary>
    /// <param name="stateId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<City>? GetCities(int stateId)
    {
        try
        {
            return _context.Cities.Where(x => x.StateId == stateId).ToList() ?? new List<City>();
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while fetching state.", ex);   
        }
    }


    /// <summary>
    /// method for getting user + profile data using email
    /// </summary>
    /// <param name="email"></param>
    /// <returns> returns EditRegisteredUserViewModel </returns>
    public EditRegisteredUserViewModel? GetUserDetailsByEmail(string email)
    {
        try
        {
            EditRegisteredUserViewModel? query = _context.Users
                                                .Include(p => p.Profile)
                                                .ThenInclude(c => c.Country)
                                                .ThenInclude(s => s.States)
                                                .ThenInclude(x => x.Cities)
                                                .Where(x => x.Email == (email.ToLower().Trim()))
                                                .Select(x => new EditRegisteredUserViewModel 
                                                {
                                                    FirstName = x.Profile.FirstName,
                                                    LastName = x.Profile.LastName,
                                                    Email = x.Email,
                                                    UserName = x.UserName,
                                                    UserId = x.UserId,
                                                    ProfileId = x.ProfileId,
                                                    RoleId  = x.RoleId,
                                                    PhoneNumber = x.Profile.PhoneNumber,
                                                    Address = x.Profile.Address,
                                                    Pincode = x.Profile.Pincode,
                                                    CountryId = x.Profile.CountryId,
                                                    StateId = x.Profile.StateId,
                                                    CityId = x.Profile.CityId,
                                                    CityName = x.Profile.City.City1,
                                                    CountryName = x.Profile.Country.Country1,
                                                    StateName = x.Profile.State.State1
                                                })
                                                .FirstOrDefault();
            
            return query ?? new EditRegisteredUserViewModel();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for getting profile based on profile id
    /// </summary>
    /// <param name="profileId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Profile? GetProfileById(int profileId)
    {
        try
        {
            return _context.Profiles.FirstOrDefault( p => p.ProfileId == profileId) ?? null;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for update profile
    /// </summary>
    /// <param name="profile"></param>
    /// <exception cref="Exception"></exception>
    public void UpdateProfile(Profile profile)
    {
        try
        {
            _context.Profiles.Update(profile);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for get user's country name
    /// </summary>
    /// <param name="countryId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string? GetCountryNameById(int countryId)
    {
        try
        {
            return _context.Countries.Where(c => c.CountryId == countryId).Select(c => c.Country1).FirstOrDefault();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for get user's state name
    /// </summary>
    /// <param name="stateId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string? GetStateNameById(int stateId)
    {
        try
        {
            return _context.States.Where(c => c.StateId == stateId).Select(c => c.State1).FirstOrDefault();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for get user's city name
    /// </summary>
    /// <param name="CityId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string? GetCityNameById(int CityId)
    {
        try
        {
            return _context.Cities.Where(c => c.CityId == CityId).Select(c => c.City1).FirstOrDefault();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for getting users by product id from favourite list
    /// </summary>
    /// <param name="productId"></param>
    /// <returns>List<User></returns>
    public List<User>? GetUsersByProductIdFromFavourite(int productId)
    {
        try
        {
            return _context.Users.Join(_context.Favourites,
                                        user => user.UserId,
                                        favourite => favourite.UserId,
                                        (user, favourite) => new { user, favourite })
                                    .Where(uf => uf.favourite.ProductId == productId)
                                    .Select(uf => uf.user)
                                    .Distinct()
                                    .ToList();
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while fetching users by product ID.", e);
        }
    }
    

    /// <summary>
    /// Method to add a contact message to the database.
    /// </summary>
    /// <param name="contact"></param>
    /// <exception cref="Exception"></exception>
    public void AddContact(Contactu contact)
    {
        try
        {
            _context.Contactus.Add(contact);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while adding contact.", e);
        }
    }

    
}