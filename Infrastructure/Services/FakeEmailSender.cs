using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

public class FakeEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // For development, just log or do nothing
        Console.WriteLine($"Email to: {email}, Subject: {subject}, Message: {htmlMessage}");
        return Task.CompletedTask;
    }
}