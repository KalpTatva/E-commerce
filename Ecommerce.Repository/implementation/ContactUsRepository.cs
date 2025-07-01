using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class ContactUsRepository : GenericRepository<Contactu> , IContactUsRepository
{
    private readonly EcommerceContext _context;
    public ContactUsRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }

}
