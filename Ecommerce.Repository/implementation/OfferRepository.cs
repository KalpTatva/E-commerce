using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class OfferRepository : GenericRepository<Offer> , IOfferRepository
{
    private readonly EcommerceContext _context;
    public OfferRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }
}
