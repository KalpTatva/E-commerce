using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    private readonly EcommerceContext _context;
    public ReviewRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }
}
