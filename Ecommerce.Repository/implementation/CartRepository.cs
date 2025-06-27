using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Repository.implementation;

public class CartRepository : ICartRepository
{
    private readonly EcommerceContext _context;

    public CartRepository(EcommerceContext context)
    {
        _context = context;
    }

    /// <summary>
    /// method for add in cart
    /// </summary>
    /// <param name="cart"></param>
    /// <exception cref="Exception"></exception>
    public void AddToCart(Cart cart)
    {
        try
        {
            _context.Carts.Add(cart);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method which gets cart data based on user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns> List<productAtCartViewModel></returns>
    public List<productAtCartViewModel> GetproductAtCart(int userId)
    {
        try
        {
            List<productAtCartViewModel>? query = (
                from product in _context.Products
                join cart in _context.Carts on product.ProductId equals cart.ProductId
                where product.IsDeleted == false && cart.IsDeleted == false &&  cart.UserId == userId
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
                }).ToList();
            
            return query;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// method for updating cart's quantity only
    /// </summary>
    /// <param name="cartId"></param>
    /// <param name="quantity"></param>
    /// <exception cref="Exception"></exception>
    public void UpdateCartById(int cartId,int quantity)
    {
        try
        {
            Cart? cart = _context.Carts.Where(c => c.CartId == cartId).FirstOrDefault();
            if(cart!=null)
            {
                cart.Quantity = quantity;
                _context.Carts.Update(cart);
                _context.SaveChanges();
            }
        
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// soft delete method for updating delete boolean = true
    /// </summary>
    /// <param name="cartId"></param>
    /// <exception cref="Exception"></exception>
    public void DeleteCartById(int cartId)
    {
        try
        {
            Cart? cart = _context.Carts.Where(c => c.CartId == cartId).FirstOrDefault();
            if(cart!=null)
            {
                cart.IsDeleted = true;
                cart.DeletedAt = DateTime.Now;
                _context.Carts.Update(cart);
                _context.SaveChanges();
            }
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// soft delete implementation for cart items 
    /// </summary>
    /// <param name="cartIds"></param>
    /// <exception cref="Exception"></exception>
    public void DeleteCartByIdsRange(List<int> cartIds)
    {
        try
        {
            List<Cart>? carts = _context.Carts.Where(c => cartIds.Contains(c.CartId)).ToList();
            if(carts!=null)
            {
                foreach(Cart cart in carts)
                {
                    cart.IsDeleted = true;
                    cart.DeletedAt = DateTime.Now;
                }
                _context.Carts.UpdateRange(carts);
                _context.SaveChanges();
            }
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// method for getting cart by user id and product id
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <returns>Cart</returns>
    public Cart? GetCartByUserIdAndProductId(int userId, int productId)
    {
        try
        {
            return _context.Carts.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId && c.IsDeleted == false);
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// get product details by cart id
    /// </summary>
    /// <param name="cartId"></param>
    /// <returns>product</returns>
    /// <exception cref="Exception"></exception>
    public Product? GetProductByCartId(int cartId, int userId)
    {
        try{
            return _context.Carts.Where(c => c.CartId == cartId && c.IsDeleted == false && c.UserId == userId)
                .Join(_context.Products,
                    cart => cart.ProductId,
                    product => product.ProductId,
                    (cart,product) => product)
                .FirstOrDefault();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
