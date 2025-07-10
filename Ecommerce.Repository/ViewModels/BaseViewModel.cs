using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.ViewModels;

public class BaseViewModel
{
    public string? BaseEmail {get;set;}
    public string? BaseRole {get;set;}
    public string? BaseUserName {get;set;}
    public string? BaseTheme {get;set;}

    public List<Notification> Notifications { get; set; } = new List<Notification>();
}
