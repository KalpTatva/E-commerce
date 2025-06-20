using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Repository.interfaces;

public interface IOrderRepository
{

    Task<List<productAtOrderViewModel>?> GetDetailsForOrders(List<int> cartIds);
    Task<List<productAtOrderViewModel>?> GetDetailsForOrdersByProductId(List<int> productId);
    void AddOrder(Order order);
    void AddOrderProductRange(List<OrderProduct> orderProducts);
}
