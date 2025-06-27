using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Ecommerce.Repository.ViewModels;

public class RegisterUserViewModel
{
    
    [Required(ErrorMessage = "username is required")]
    [MaxLength(50,ErrorMessage = "limit exceed ")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens.")]
    public string UserName { get; set; } 

    [Required(ErrorMessage = "role is required")]
    public int RoleId { get; set; }
    
    [Required(ErrorMessage = "firstname is required")]
    [MaxLength(40,ErrorMessage = "limit exceed ")] ///^[a-zA-Z]*$/
    [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "name dosen't contain special case")]
    public string FirstName {get;set;}
    
    [Required(ErrorMessage = "lastname is required")]
    [MaxLength(40,ErrorMessage = "limit exceed ")]
    [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "name dosen't contain special case")]
    public string LastName {get;set;}

    [Required(ErrorMessage = "email is required")]
    [MaxLength(50,ErrorMessage = "limit exceed ")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email is not valid")]
    public string Email { get; set; } 

    [MaxLength(50,ErrorMessage = "limit exceed ")]
    [Required(ErrorMessage = "password is required")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Atleast contain 1-uppercase, 1-lowercase, 1-special charecter, 1-number  and length should be 8")]
    public string Password { get; set; } 

    [Required(ErrorMessage = "Confirm Password is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword {get;set;}

    [Required(ErrorMessage = "phone number is required")]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid phone number, should be exactly 10 digits.")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "address is required")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "zipcode is required")]
    [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Invalid pincode, should be exactly 6 digits.")]
    public int Pincode { get; set; }

    [Required(ErrorMessage = "country is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Select a country")]
    public int CountryId { get; set; }

    [Required(ErrorMessage = "state is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Select a state")]
    public int StateId { get; set; }

    [Required(ErrorMessage = "city is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Select a city")]
    public int CityId { get; set; }

}
