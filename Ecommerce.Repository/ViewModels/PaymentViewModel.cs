namespace Ecommerce.Repository.ViewModels;

public class PaymentViewModel
{
    public string? orderId {get;set;}
    public string? sessionId { get; set; }
    public int UserId { get; set; }
    public string? RazorpayKey { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalQuantity { get; set; }
    public string? Currency { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
}
