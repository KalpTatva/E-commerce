using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class PasswordResetRequestRepository : GenericRepository<PasswordResetRequest>, IPasswordResetRequestRepository
{
    private readonly EcommerceContext _context;
    public PasswordResetRequestRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }
}
