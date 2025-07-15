using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class CityRepository : GenericRepository<City>, ICityRepository
{
    private readonly EcommerceContext _context;
    public CityRepository(EcommerceContext context) : base (context)
    {
        _context = context;
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
}
