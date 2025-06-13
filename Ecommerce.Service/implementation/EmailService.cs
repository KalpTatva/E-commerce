using System.Net;
using System.Net.Mail;

namespace Ecommerce.Service.interfaces.implementation;

public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try{
            using var mail = new MailMessage();
            mail.From = new MailAddress("test.dotnet@etatvasoft.com", "Kalp pandya");
            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            using var smtp = new SmtpClient("mail.etatvasoft.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("test.dotnet@etatvasoft.com", "P}N^{z-]7Ilp"),
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
