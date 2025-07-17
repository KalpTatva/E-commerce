using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.ViewModels;

public class SellersViewModel
{
    public List<UserViewmodel>? users {get;set;}
    public List<Category>? categories {get;set;}
}

public class UserViewmodel
{
    public int UserId {get;set;}
    public string? UserName {get;set;}
    public string? Email {get;set;}
    public List<GrantOfferPermission>? grantOfferPermissions {get;set;}  
}

public class GrantIdsViewModel
{
    public int userId {get;set;}
    public int categoryId {get;set;}
}
 