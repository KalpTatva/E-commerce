using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class FavouriteRepository : IFavouriteRepository
{
    private readonly EcommerceContext _context;

    public FavouriteRepository(EcommerceContext context)
    {
        _context = context;
    }

    /// <summary>
    /// method for fetch favourites by user and product id
    /// </summary>
    /// <param name="UserId"></param>
    /// <param name="ProductId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Favourite? GetFavouriteByIds(int UserId,int ProductId)
    {
        try
        {
            return _context.Favourites.FirstOrDefault(f => f.UserId == UserId && f.ProductId == ProductId);
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// method for dropping favourite tupple from db
    /// </summary>
    /// <param name="favourite"></param>
    /// <exception cref="Exception"></exception>
    public void dropFavourite(Favourite favourite)
    {
        try
        {
            _context.Favourites.Remove(favourite);
            _context.SaveChanges();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }



    /// <summary>
    /// method for add tupple in favourite
    /// </summary>
    /// <param name="favourite"></param>
    /// <exception cref="Exception"></exception>
    public void AddFavourite(Favourite favourite)
    {
        try
        {
            _context.Favourites.Add(favourite);
            _context.SaveChanges();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for get favourite tupples from db by user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
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
