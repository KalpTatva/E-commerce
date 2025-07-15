using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class OrderProductRepository : GenericRepository<OrderProduct> , IOrderProductRepository
{
    private readonly EcommerceContext _context;

    public OrderProductRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }
}
