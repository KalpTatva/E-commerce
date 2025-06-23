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


    /// <summary>
    /// Get details for orders based on cart IDs.
    /// This method retrieves product details for the specified cart IDs, 
    /// including product name, price, stocks, discount type, discount amount, quantity, and associated images.
    /// </summary>
    /// <param name="cartIds"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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

    /// <summary>
    /// Get details for orders based on product IDs.
    /// This method retrieves product details for the specified product IDs,
    /// including product name, price, stocks, discount type, discount amount, 
    /// quantity, and associated images.
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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

    /// <summary>
    /// Method to add an order to the database.
    /// This method adds a new order to the Orders table and 
    /// saves changes to the database.
    /// </summary>
    /// <param name="order"></param>
    /// <exception cref="Exception"></exception>
    public void AddOrder(Order order)
    {
        try
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Method to add a range of order products to the database.
    /// This method adds multiple order products to the OrderProducts table and
    /// saves changes to the database.
    /// </summary>
    /// <param name="orderProducts"></param>
    /// <exception cref="Exception"></exception>
    public void AddOrderProductRange(List<OrderProduct> orderProducts)
    {
        try
        {
            _context.OrderProducts.AddRange(orderProducts);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Method to get the order details for a specific user.
    /// This method retrieves a list of orders placed by the user, including order items and their details.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>It returns a list of MyOrderViewModel objects containing order and item details.</returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<MyOrderViewModel>?> GetMyOrderDetails(int userId)
    {
        try
        {
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
                            Status = op.Status,
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


    /// <summary>
    /// Method to get the seller's orders.
    /// This method retrieves a list of orders placed for products sold by the seller, including order details and buyer information.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>It returns a list of SellerOrderViewModel objects containing order and buyer details.</returns>
    /// <exception cref="Exception"></exception>
    public List<SellerOrderViewModel> GetSellerOrders(int userId)
    {
        try
        {
            List<SellerOrderViewModel> query = _context.OrderProducts
                .Where(op => op.Product.SellerId == userId)
                .OrderByDescending(op => op.OrderProductId)
                .Join(_context.Orders,
                    op => op.OrderId,
                    order => order.OrderId,
                    (op, order) => new SellerOrderViewModel
                    {
                        OrderId = op.OrderProductId,
                        ProductName = op.Product.ProductName,
                        Quantity = op.Quantity,
                        Stocks = op.Product.Stocks,
                        Price = op.PriceWithDiscount,
                        BuyerEmail = order.Buyer.Email,
                        OrderDate = order.CreatedAt ?? DateTime.Now,
                        OrderStatus = op.Status,
                        BuyerName =
                            _context.Users
                                .Where(u => u.UserId == order.BuyerId)
                                .Join(_context.Profiles,
                                    u => u.ProfileId,
                                    ud => ud.ProfileId,
                                    (u, ud) => ud)
                                .Select(u => u.FirstName + " " + u.LastName)
                                .FirstOrDefault(),
                        Address =
                            _context.Users
                                .Where(u => u.UserId == order.BuyerId)
                                .Join(_context.Profiles,
                                    u => u.ProfileId,
                                    ud => ud.ProfileId,
                                    (u, ud) => ud.Address + ", " +
                                              ud.City.City1 + ", " +
                                              ud.State.State1 + ", " +
                                              ud.Country.Country1 + ", " +
                                              ud.Pincode)
                                .FirstOrDefault()

                    })
                .ToList();

            return query;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// Method to get an order by its ID.
    /// This method retrieves a specific order from the OrderProducts table based on the provided order ID.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns>It returns an OrderProduct object if found, otherwise null.</returns>
    public OrderProduct? GetOrderById(int orderId)
    {
        try
        {
            OrderProduct? order = 
                _context.OrderProducts
                .FirstOrDefault(op => op.OrderProductId == orderId && op.IsDeleted == false);
            return order;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Method to update an existing order product.
    /// This method updates the details of an order product in the OrderProducts table
    /// and saves the changes to the database.
    /// </summary>
    /// <param name="order"></param>
    public void UpdateOrderProducts(OrderProduct order)
    {
        try
        {
            _context.OrderProducts.Update(order);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

}
