namespace Ecommerce.Repository.ViewModels;

public class ResponsesViewModel
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
}

public class ResponseTokenViewModel
{
    public string? token { get; set; }
    public string? response { get; set;}
    public bool isPersistent { get; set;} 
    public string? Role { get; set; } 
    public string? sessionId {get;set;}
    public string? UserName {get;set;}
    public string? BaseTheme {get; set;}
}