using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Repository.ViewModels;

public class SearchInputViewModel : BaseViewModel
{

    [StringLength(2048, ErrorMessage = "Search input cannot exceed 2048 characters.")]
    public string? SearchInput {get; set;}
}
