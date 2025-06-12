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
}
