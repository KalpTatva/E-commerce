using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface IFavouriteRepository
{
    /// <summary>
    /// method for fetch favourites by user and product id
    /// </summary>
    /// <param name="UserId"></param>
    /// <param name="ProductId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Favourite? GetFavouriteByIds(int UserId,int ProductId);

    /// <summary>
    /// method for dropping favourite tupple from db
    /// </summary>
    /// <param name="favourite"></param>
    /// <exception cref="Exception"></exception>
    public void dropFavourite(Favourite favourite);

    /// <summary>
    /// method for add tupple in favourite
    /// </summary>
    /// <param name="favourite"></param>
    /// <exception cref="Exception"></exception>
    public void AddFavourite(Favourite favourite);

    /// <summary>
    /// method for get favourite tupples from db by user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<int> GetFavouriteByUserId(int userId);
}
