using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class FeatureRepository : GenericRepository<Feature>, IFeatureRepository
{

    private readonly EcommerceContext _context;

    public FeatureRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }
}
