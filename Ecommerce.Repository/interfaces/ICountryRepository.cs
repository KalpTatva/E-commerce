using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface ICountryRepository : IGenericRepository<Country>
{
    /// <summary>
    /// method for get user's country name
    /// </summary>
    /// <param name="countryId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    string? GetCountryNameById(int countryId);
}
