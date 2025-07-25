namespace Ecommerce.Repository.ViewModels;

public class DashBoardViewModel
{
    public List<PriceAndDateViewModel>? priceAndDate {get;set;}
    public List<CountAndProductwithImageViewModel>? TopSellingProduct {get;set;}
    public List<CountAndProductwithImageViewModel>? LeastSellingProduct {get;set;}
    public List<CountAndDatewithImageViewModel>? CustomersData {get;set;}

}

public class PriceAndDateViewModel
{
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public string dateNumber {get;set;} = "";
}


public class CountAndProductwithImageViewModel
{
    public decimal Count { get; set; }
    public int productId {get;set;}
    public string? productName { get; set; }
    public string? ImageUrl { get; set; }
}


public class CountAndDatewithImageViewModel
{
    public decimal CustomerCount { get; set; }
    public DateTime Date { get; set; }
    public string dateNumber {get;set;} = "";
}