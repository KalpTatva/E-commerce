namespace Ecommerce.Repository.ViewModels;

public class DashBoardViewModel
{
    public List<PriceAndDateViewModel>? priceAndDate {get;set;}
    public List<CountAndDatewithImageViewModel>? TopSellingProduct {get;set;}
    public List<CountAndDatewithImageViewModel>? LeastSellingProduct {get;set;}
}

public class PriceAndDateViewModel
{
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public string dateNumber {get;set;} = "";
}


public class CountAndDatewithImageViewModel
{
    public decimal Count { get; set; }
    public int productId {get;set;}
    public string? productName { get; set; }
    public string? ImageUrl { get; set; }
}