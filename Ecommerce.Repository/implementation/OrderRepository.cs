using System.Threading.Tasks;
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

    public async Task<List<MyOrderViewModel>?> GetMyOrderDetails(int userId)
    {
        try
        {
            // List<MyOrderViewModel>? query = await (
            //     from order in _context.Orders
            //     where order.BuyerId == userId
            //     select new MyOrderViewModel
            //     {
            //         OrderId = order.OrderId,
            //         BuyerId = order.BuyerId,
            //         Amount = order.Amount,
            //         Status = order.Status,
            //         CreatedAt = order.CreatedAt,
            //         TotalDiscount = order.TotalDiscount,
            //         TotalQuantity = order.TotalQuantity,
            //         OrderedItem = (
            //             from orderProduct in _context.OrderProducts
            //             join product in _context.Products on orderProduct.ProductId equals product.ProductId
            //             where orderProduct.OrderId == order.OrderId
            //             select new OrderItemsViewModel
            //             {
            //                 OrderProductId = orderProduct.OrderProductId,
            //                 OrderId = orderProduct.OrderId,
            //                 ProductName = product.ProductName,
            //                 ProductId = orderProduct.ProductId,
            //                 Quantity = orderProduct.Quantity,
            //                 PriceWithDiscount = orderProduct.PriceWithDiscount,
            //                 CreatedAt = orderProduct.CreatedAt
            //             }
            //         ).ToList()
            //     }
            // ).ToListAsync();

            List<MyOrderViewModel> query = await _context.Orders
            .Where(order => order.BuyerId == userId)
            .Select(order => new MyOrderViewModel
            {
                OrderId = order.OrderId,
                BuyerId = order.BuyerId,
                Amount = order.Amount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                TotalDiscount = order.TotalDiscount,
                TotalQuantity = order.TotalQuantity,
                OrderedItem = _context.OrderProducts
                    .Where(op => op.OrderId == order.OrderId)
                    .Join(_context.Products,
                        op => op.ProductId,
                        product => product.ProductId,
                        (op, product) => new OrderItemsViewModel
                        {
                            OrderProductId = op.OrderProductId,
                            OrderId = op.OrderId,
                            ProductName = product.ProductName,
                            ProductId = op.ProductId,
                            Quantity = op.Quantity,
                            PriceWithDiscount = op.PriceWithDiscount,
                            CreatedAt = op.CreatedAt
                        })
                    .ToList()
            })
            .ToListAsync();

            return query;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

}
