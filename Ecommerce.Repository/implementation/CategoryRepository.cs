using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    private readonly EcommerceContext _context;

    public CategoryRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }
}
