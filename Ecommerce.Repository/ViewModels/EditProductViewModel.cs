using System.ComponentModel.DataAnnotations;
using Ecommerce.Repository.Models;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Repository.ViewModels;

public class EditProductViewModel : BaseViewModel
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "product name is required")]
    public string ProductName { get; set; } = null!;

    [Required(ErrorMessage = "product description is required")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "please select the category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "price must be greater than zero")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "price must be a valid number with up to two decimal places")]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "stocks is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "stocks must be greater than zero")]
    public int Stocks { get; set; }
    public int SellerId { get; set; }

    
    public List<Feature>? Features {get;set;}

    public List<Image>? Images {get;set;}  

    public List<IFormFile>? imageFile { get; set; }

    public string? FeaturesInput { get; set; }
    public string? ImageDeleteInput { get; set; }

    [Required(ErrorMessage = "select discount type")]
    public int? DiscountType { get; set; }

    [Required(ErrorMessage = "Discount is required")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "discount must be a valid number with up to two decimal places")]
    public decimal? Discount { get; set; }
}
