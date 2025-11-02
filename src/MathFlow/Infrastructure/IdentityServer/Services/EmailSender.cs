using Microsoft.AspNetCore.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;
using System.Net.Mail;

namespace MathFlow.Infrastructure.IdentityServer.Services;

/// <summary>
/// Email sender implementation for ASP.NET Core Identity.
/// In development, emails are sent to Mailpit (SMTP mock).
/// In production, configure with real SMTP provider.
/// </summary>
public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        return SendEmailAsync(email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        return SendEmailAsync(email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        return SendEmailAsync(email, "Reset your password",
            $"Please reset your password using the following code: {resetCode}");
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var smtpHost = _configuration["Email:Smtp:Host"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["Email:Smtp:Port"] ?? "1025");
        var smtpUser = _configuration["Email:Smtp:User"] ?? "";
        var smtpPassword = _configuration["Email:Smtp:Password"] ?? "";
        var fromEmail = _configuration["Email:From"] ?? "noreply@mathflow.local";

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort);
            client.EnableSsl = false;

            if (!string.IsNullOrEmpty(smtpUser))
            {
                client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "MathFlow"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent to {Email} with subject '{Subject}'", email, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
        }
    }
}
