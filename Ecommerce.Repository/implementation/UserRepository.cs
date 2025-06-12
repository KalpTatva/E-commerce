

using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class UserRepository : IUserRepository
{
    private readonly EcommerceContext _context;

    public UserRepository(EcommerceContext context)
    {
        _context = context;
    }


    /// <summary>
    /// function for getting perticular user by email
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
}
