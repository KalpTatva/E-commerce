using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Repository.ViewModels;

public class EditRegisteredUserViewModel : BaseViewModel
{
    
    public string UserName { get; set; } = null!;

    public int RoleId { get; set; }
    
    public string Email { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int UserId {get;set;} 
    public int ProfileId {get;set;}

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
    public int? CountryId { get; set; }

    [Required(ErrorMessage = "state is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Select a state")]
    public int? StateId { get; set; }

    [Required(ErrorMessage = "city is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Select a city")]
    public int? CityId { get; set; }
    public string? CityName {get;set;}
    public string? CountryName {get;set;}
    public string? StateName {get;set;}
}
