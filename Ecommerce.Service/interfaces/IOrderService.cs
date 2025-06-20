using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Service.interfaces;

public interface IOrderService
{
    Task<OrderViewModel> GetDetailsForOrder(ObjectSessionViewModel obj, string email);

    Task<OrderViewModel> GetDetailsForSingleOrder(ObjectSessionViewModel obj, string email);

    Task<ResponsesViewModel?> PlaceOrder(ObjectSessionViewModel objSession, int UserId, bool isByProductId = false);
}
