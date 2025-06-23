using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.ViewModels;

public class CartViewModel
{
    public List<productAtCartViewModel>? ProductsAtCart {get;set;}

    public decimal TotalPrice {get;set;}
    public decimal TotalDiscount {get;set;}
    public int TotalQuantity {get;set;}
}

public class productAtCartViewModel
{
    public int CartId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal Price { get; set; }

    public int Stocks { get; set; }

    public int? DiscountType { get; set; }

    public decimal? Discount { get; set; }

    public Image? Images {get;set;} 

}

public class CartUpdatesViewModel
{
    public bool IsSuccess {get;set;}
    public decimal TotalQuantity {get;set;}
    public decimal TotalDiscount {get;set;}
    public decimal TotalPrice {get;set;}
}