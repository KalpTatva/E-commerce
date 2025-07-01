using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Repository.interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// function for getting perticular user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    User? GetUserByEmail(string email);


    /// <summary>
    /// method for getting user + profile data using email
    /// </summary>
    /// <param name="email"></param>
    /// <returns> returns EditRegisteredUserViewModel </returns>
    EditRegisteredUserViewModel? GetUserDetailsByEmail(string email);

    /// <summary>
    /// method for getting users by product id from favourite list
    /// </summary>
    /// <param name="productId"></param>
    /// <returns>List<User></returns>
    List<User>? GetUsersByProductIdFromFavourite(int productId);    
}
