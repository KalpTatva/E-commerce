using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repository.implementation;

public class UserRepository : GenericRepository<User> ,IUserRepository
{
    private readonly EcommerceContext _context;

    public UserRepository(EcommerceContext context) : base (context)
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
                                                .Where(x => x.Email == email)
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
    
}