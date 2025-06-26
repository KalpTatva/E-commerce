using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.ViewModels;

public class ObjectSessionViewModel
{
    public decimal totalPrice {get;set;}
    public decimal totalDiscount {get;set;}
    public decimal totalQuantity {get;set;}
    public bool isByProductId {get;set;}
   
    // contains list of cartids for multiple product to buy from cart
    // or productid for single product to buy
    public List<int>? orders {get;set;}
}

public class OrderViewModel : BaseViewModel
{
    public List<productAtOrderViewModel>? ordersList {get;set;}
    public int UserId {get;set;}
    public string? SessionId {get;set;}
    public string? FirstName {get;set;}
    public string? LastName {get;set;}
    public string? Address {get;set;}
    public string? Phone {get;set;}
    public string? CityName {get;set;}
    public string? CountryName {get;set;}
    public string? StateName {get;set;}
    public int PinCode {get;set;}
    public ObjectSessionViewModel? objSession {get;set;}
}


public class productAtOrderViewModel
{
    public int CartId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal Price { get; set; }

    public int Stocks { get; set; }

    public int? DiscountType { get; set; }

    public decimal? Discount { get; set; }

    public Offer? Offer { get; set; }
    public Image? Images {get;set;} 

}
