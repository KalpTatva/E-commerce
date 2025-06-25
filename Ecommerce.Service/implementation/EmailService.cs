using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Service.interfaces.implementation;

public class EmailService : IEmailService
{

    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    /// <summary>
    /// Method to send email asynchronously
    /// </summary>
    /// <param name="toEmail">Recipient's email address</param>
    /// <param name="subject">Subject of the email</param>
    /// <param name="body">Body content of the email</param>
    /// <exception cref="Exception">Throws an exception if email sending fails</exception>
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try{
            using var mail = new MailMessage();
            mail.From = new MailAddress(_configuration["EmailSettings:User"], _configuration["EmailSettings:Issuer"]);
            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            using var smtp = new SmtpClient(_configuration["EmailSettings:Host"])
            {
                Port = 587,
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:User"],
                    _configuration["EmailSettings:Password"]
                ),
                EnableSsl = true,
            };

            await smtp.SendMailAsync(mail);
        }
        catch(Exception ex)
        {
            throw new Exception("Error sending email", ex);
        }
    }
}
