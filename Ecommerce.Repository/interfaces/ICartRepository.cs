using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Repository.interfaces;

public interface ICartRepository : IGenericRepository<Cart>
{
    Task<List<productAtCartViewModel>> GetproductAtCart(int userId);
    Task<Product?> GetProductByCartId(int cartId, int userId);
}
