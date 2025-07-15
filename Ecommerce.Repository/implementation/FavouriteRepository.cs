using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class FavouriteRepository : GenericRepository<Favourite> , IFavouriteRepository
{
    private readonly EcommerceContext _context;

    public FavouriteRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }

    /// <summary>
    /// method for get favourite tupples from db by user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>List<int></returns>
    /// <exception cref="Exception"></exception>
    public List<int> GetFavouriteByUserId(int userId)
    {
        try
        {
            return _context.Favourites.Where(f => f.UserId == userId).Select(x => x.ProductId).ToList();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
