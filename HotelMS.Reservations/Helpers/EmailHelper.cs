using MailKit.Net.Smtp;
using MimeKit;

namespace HotelMS.Reservations.Helpers;

public interface IEmailHelper
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailHelper : IEmailHelper
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailHelper> _logger;

    public EmailHelper(IConfiguration config, ILogger<EmailHelper> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var host = _config["Smtp:Host"];
            var port = _config.GetValue<int>("Smtp:Port", 587);
            var senderEmail = _config["Smtp:SenderEmail"] ?? "reservations@hotelms.com";
            var senderName = _config["Smtp:SenderName"] ?? "HotelMS Reservations";

            if (string.IsNullOrEmpty(host))
            {
                _logger.LogInformation("SMTP host not configured. Email to {ToEmail} simulated. Subject: {Subject}", toEmail, subject);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.Auto);
            var user = _config["Smtp:Username"];
            var pass = _config["Smtp:Password"];
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                await client.AuthenticateAsync(user, pass);
            }
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {ToEmail}. Fallback simulation logged.", toEmail);
        }
    }
}
