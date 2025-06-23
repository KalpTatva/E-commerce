using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Repository.ViewModels;

public class ForgetPasswordViewModel
{
    public string? Email { get; set; }
    public string? Token { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Atleast contain 1-uppercase, 1-lowercase, 1-special charecter, 1-number  and length should be 8")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Confirm Password is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; }
}
