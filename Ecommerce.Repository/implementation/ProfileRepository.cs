using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class ProfileRepository : GenericRepository<Profile> , IProfileRepository
{
    private readonly EcommerceContext _context;

    public ProfileRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }
}
