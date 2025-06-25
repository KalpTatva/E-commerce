using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Repository.ViewModels;

public class ContactUsViewModel : BaseViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(100,ErrorMessage = "limit exceed ")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid user credentials")]
    public string SenderEmail { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [MaxLength(100,ErrorMessage = "limit exceed ")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid user credentials")]
    public string ReciverEmail { get; set; } = null!;

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Subject is required")]
    public string Subject { get; set; } = null!;

    [Required(ErrorMessage = "Message is required")]
    public string Message { get; set; } = null!;

    [Required(ErrorMessage = "Product ID is required")]
    public int ProductId { get; set; } = 0;
}
