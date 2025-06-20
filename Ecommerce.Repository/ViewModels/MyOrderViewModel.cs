namespace Ecommerce.Repository.ViewModels;


public class OrderAtMyOrderViewModel : BaseViewModel{
    public List<MyOrderViewModel>? myOrderViewModels {get;set;}
}

public class MyOrderViewModel
{
    public int OrderId { get; set; }

    public int? BuyerId { get; set; }

    public decimal Amount { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }
    
    public int TotalQuantity { get; set; }

    public decimal TotalDiscount { get; set; }

    public List<OrderItemsViewModel>? OrderedItem {get;set;} 
}

public class OrderItemsViewModel
{
    public int OrderProductId { get; set; }

    public int OrderId { get; set; }
    
    public string? ProductName {get;set;}   

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceWithDiscount { get; set; }

    public DateTime? CreatedAt { get; set; }
}
