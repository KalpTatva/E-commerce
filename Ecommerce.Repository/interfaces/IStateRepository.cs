using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface IStateRepository : IGenericRepository<State>
{
    /// <summary>
    /// method for get user's state name
    /// </summary>
    /// <param name="stateId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    string? GetStateNameById(int stateId);
}
