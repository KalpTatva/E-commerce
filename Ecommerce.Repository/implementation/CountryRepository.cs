using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class CountryRepository : GenericRepository<Country> , ICountryRepository
{
    private readonly EcommerceContext _context;
    public CountryRepository(EcommerceContext context) : base (context)
    {
        _context = context;
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
}
