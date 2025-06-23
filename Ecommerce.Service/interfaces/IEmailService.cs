namespace Ecommerce.Service.interfaces;

public interface IEmailService
{
    
    /// <summary>
    /// Method to send email asynchronously
    /// </summary>
    /// <param name="toEmail">Recipient's email address</param>
    /// <param name="subject">Subject of the email</param>
    /// <param name="body">Body content of the email</param>
    /// <exception cref="Exception">Throws an exception if email sending fails</exception>
    Task SendEmailAsync(string toEmail, string subject, string body);
}
