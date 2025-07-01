using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class UserNotificationMappingRepository : GenericRepository<UserNotificationMapping>, IUserNotificationMappingRepository
{
    private readonly EcommerceContext _context;
    public UserNotificationMappingRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }
}
