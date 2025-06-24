using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.ViewModels;

public class ProductsViewModel : BaseViewModel
{
    public List<ProductsDeatailsViewModel>? productsDetails {get;set;}
    public List<int>? favourites {get;set;}
}

public class ProductsDeatailsViewModel
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal Price { get; set; }

    public int Stocks { get; set; }

    public int SellerId { get; set; }

    public int? DiscountType { get; set; }

    public decimal? Discount { get; set; }

    public bool OfferAvailable { get; set; }

    public decimal AverageRatings { get; set; }

    public List<Feature>? Features {get;set;}
    public Offer? offer {get;set;}


    public Image? Images {get;set;} 
}


public class productDetailsByproductIdViewModel : BaseViewModel
{
    public int ProductId { get; set; }
    public string? UserEmail {get;set;}

    public string ProductName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal Price { get; set; }

    public int Stocks { get; set; }

    public int SellerId { get; set; }

    public bool IsFavourite {get;set;}

    public int? DiscountType { get; set; }

    public decimal? Discount { get; set; }

    public List<Feature>? Features {get;set;}

    public List<ReviewsViewModel>? Reviews {get;set;}

    public Offer? offer {get;set;}
    public decimal AverageRatings { get; set; }

    public List<Image>? Images {get;set;} 
}

public class ReviewsViewModel{
    public int ReviewId { get; set; }
    public string UserName { get; set; } = null!;
    public string? UserEmail { get; set; }
    public decimal Ratings { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
}