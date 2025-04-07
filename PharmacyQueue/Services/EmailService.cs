using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;

namespace PharmacyQueue.Services
{
    // Service responsible for sending emails in the pharmacy queue system.
    // Handles email notifications for appointments, reminders, and status updates.
    public class EmailService
    {
        // Configuration service to access email settings from appsettings.json
        private readonly IConfiguration _configuration;
        // Logger service for tracking email operations and errors
        private readonly ILogger<EmailService> _logger;

        // Initializes a new instance of the EmailService.
        // configuration: Application configuration service
        // logger: Logging service for tracking operations
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        // Throws InvalidOperationException when email settings are not properly configured
        // Throws Exception when email sending fails
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Retrieve email configuration settings from appsettings.json
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"].ToString());
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var password = _configuration["EmailSettings:SenderPassword"];

                // Validate required email settings
                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
                {
                    _logger.LogError("Email settings are not properly configured");
                    throw new InvalidOperationException("Email settings are not properly configured");
                }

                // Configure SMTP client with settings from configuration
                using var client = new SmtpClient()
                {
                    Host = smtpHost,
                    Port = smtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail, password)
                };

                // Create and configure the email message
                using var message = new MailMessage()
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                // Add recipient to the message
                message.To.Add(new MailAddress(to));

                // Log the attempt to send email
                _logger.LogInformation($"Attempting to send email to {to}");
                // Send the email asynchronously
                await client.SendMailAsync(message);
                // Log successful email sending
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                // Log any errors that occur during email sending
                _logger.LogError(ex, $"Failed to send email to {to}. Error: {ex.Message}");
                throw;
            }
        }
    }
}