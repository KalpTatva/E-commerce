using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface IFavouriteRepository : IGenericRepository<Favourite>
{

    /// <summary>
    /// method for get favourite tupples from db by user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<int> GetFavouriteByUserId(int userId);
}
