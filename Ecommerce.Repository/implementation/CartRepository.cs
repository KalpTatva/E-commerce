using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repository.implementation;

public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    private readonly EcommerceContext _context;

    public CartRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }

   public async Task<List<productAtCartViewModel>> GetproductAtCart(int userId)
    {
        try
        {
            List<productAtCartViewModel> query = await (
                from product in _context.Products
                join cart in _context.Carts on product.ProductId equals cart.ProductId
                where product.IsDeleted == false && cart.IsDeleted == false && cart.UserId == userId
                orderby cart.CartId descending
                select new productAtCartViewModel
                {
                    CartId = cart.CartId,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Stocks = product.Stocks,
                    Quantity = cart.Quantity,
                    DiscountType = product.DiscountType,
                    Discount = product.Discount,
                    Images = _context.Images.Where(i => i.ProductId == product.ProductId).OrderBy(i => i.ImageId).FirstOrDefault(),
                    Offer = _context.Offers
                        .Where(o => o.ProductId == product.ProductId && 
                                    o.StartDate.Date <= DateTime.Now.Date && 
                                    o.EndDate.Date > DateTime.Now.Date)
                        .FirstOrDefault()
                }).ToListAsync();
            
            return query;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving cart products: {e.Message}");
        }
    }

    public async Task<Product?> GetProductByCartId(int cartId, int userId)
    {
        try
        {
            return await _context.Carts
                .Where(c => c.CartId == cartId && c.IsDeleted == false && c.UserId == userId)
                .Join(_context.Products,
                    cart => cart.ProductId,
                    product => product.ProductId,
                    (cart, product) => product)
                .FirstOrDefaultAsync();
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving product by cart ID: {e.Message}");
        }
    }

}
