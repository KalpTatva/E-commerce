using System.ComponentModel.DataAnnotations;
using Ecommerce.Repository.CustomValidation;

namespace Ecommerce.Repository.ViewModels;

[DateDifference(ErrorMessage = "Start date should not be after end date.")]
public class OfferViewModel : BaseViewModel
{
    public int OfferId { get; set; } 

    public int ProductId { get; set; }

    [Required(ErrorMessage = "Offer type is required.")]
    [Range(1, 3, ErrorMessage = "Offer type must be between 1 and 3.")]
    public int OfferType { get; set; }

    [Required(ErrorMessage = "Discount rate is required.")]
    public decimal? DiscountRate { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "End date is required.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);

    public DateTime? CreatedAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public string? StartTime {get;set;}
    
    public string? EndTime {get;set;}
}


public class ProductNameViewModel
{
    public int id {get;set;}
    public string? name {get;set;}
}


