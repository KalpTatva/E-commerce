using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface ICityRepository : IGenericRepository<City>
{
    /// <summary>
    /// method for get user's city name
    /// </summary>
    /// <param name="CityId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    string? GetCityNameById(int CityId);
}
