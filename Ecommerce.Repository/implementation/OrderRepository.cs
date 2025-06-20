using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repository.implementation;

public class OrderRepository : IOrderRepository
{
    private readonly EcommerceContext _context;

    public OrderRepository(EcommerceContext context)
    {
        _context = context;
    }


    public async Task<List<productAtOrderViewModel>?> GetDetailsForOrders(List<int> cartIds)
    {
        try
        {
            List<productAtOrderViewModel>? query = await (
                from product in _context.Products
                join cart in _context.Carts on product.ProductId equals cart.ProductId
                where cartIds.Contains(cart.CartId) && product.IsDeleted == false
                select new productAtOrderViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Stocks = product.Stocks,
                    DiscountType = product.DiscountType,
                    Discount = product.Discount,
                    Quantity = cart.Quantity,
                    Images = _context.Images.Where(i => i.ProductId == product.ProductId).OrderBy(i => i.ImageId).FirstOrDefault()
                }).ToListAsync();

            return query;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<List<productAtOrderViewModel>?> GetDetailsForOrdersByProductId(List<int> productId)
    {
        try
        {
            List<productAtOrderViewModel>? query = await (
                from product in _context.Products
                where product.IsDeleted == false && product.ProductId == productId.First()
                select new productAtOrderViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Stocks = product.Stocks,
                    DiscountType = product.DiscountType,
                    Discount = product.Discount,
                    Quantity = 1,
                    Images = _context.Images.Where(i => i.ProductId == product.ProductId).OrderBy(i => i.ImageId).FirstOrDefault()
                }).ToListAsync();

            return query;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    public void AddOrder(Order order)
    {
        try
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public void AddOrderProductRange(List<OrderProduct> orderProducts)
    {
         try
        {
            _context.OrderProducts.AddRange(orderProducts);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

}
