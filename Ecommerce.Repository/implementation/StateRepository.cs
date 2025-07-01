using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class StateRepository : GenericRepository<State> , IStateRepository
{
    private readonly EcommerceContext _context;
    public StateRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }

    /// <summary>
    /// method for get user's state name
    /// </summary>
    /// <param name="stateId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string? GetStateNameById(int stateId)
    {
        try
        {
            return _context.States.Where(c => c.StateId == stateId).Select(c => c.State1).FirstOrDefault();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
