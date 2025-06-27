namespace Ecommerce.Repository.ViewModels;

public class SellerOrderListViewModel : BaseViewModel
{
    public int TotalCount { get; set; }
    public List<SellerOrderViewModel> SellerOrders { get; set; } = new List<SellerOrderViewModel>();
}
public class SellerOrderViewModel
{
    public int OrderId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public int Stocks { get; set; }
    public decimal Price { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerName { get; set; }
    public DateTime OrderDate { get; set; }
    public int OrderStatus { get; set; }
    public string? Address {get;set;}
}
