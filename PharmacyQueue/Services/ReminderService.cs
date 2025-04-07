using Microsoft.EntityFrameworkCore;
using PharmacyQueue.Data;
using PharmacyQueue.Models;

namespace PharmacyQueue.Services
{
    /// <summary>
    /// Background service that handles sending email reminders to customers about their appointments.
    /// Runs continuously to check for appointments that need reminders and sends them via email.
    /// </summary>
    public class ReminderService : BackgroundService
    {
        // Service provider for dependency injection
        private readonly IServiceProvider _services;
        // Logger for tracking reminder operations and errors
        private readonly ILogger<ReminderService> _logger;

        /// <summary>
        /// Initializes a new instance of the ReminderService.
        /// </summary>
        /// <param name="services">Service provider for dependency injection</param>
        /// <param name="logger">Logger for tracking operations</param>
        public ReminderService(IServiceProvider services, ILogger<ReminderService> logger)
        {
            _services = services;
            _logger = logger;
        }

        /// Executes the background service continuously.
        /// Checks for appointments needing reminders every minute.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check and send reminders for all waiting appointments
                    await CheckAndSendReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending reminders");
                }

                // Wait for 1 minute before checking again
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        /// Checks all waiting appointments and sends reminders if needed.
        /// Sends 10-minute and 5-minute reminders based on estimated wait times.
        private async Task CheckAndSendReminders()
        {
            // Create a new scope for dependency injection
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();
            var queueService = scope.ServiceProvider.GetRequiredService<QueueService>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            // Get all appointments that are currently waiting
            var waitingAppointments = await context.Appointments
                .Where(a => (AppointmentStatus)a.Status == AppointmentStatus.Waiting)
                .ToListAsync();

            foreach (var appointment in waitingAppointments)
            {
                // Get current queue status for the appointment
                var queueStatus = await queueService.GetQueueStatus(appointment.QueueNumber);

                // Check if we need to send a 10-minute reminder
                var tenMinReminder = await context.Notifications
                    .FirstOrDefaultAsync(n => n.AppointmentID == appointment.AppointmentID &&
                                            n.Type == "10MinuteReminder");

                // Send 10-minute reminder if not sent before and wait time is <= 10 minutes
                if (tenMinReminder == null && queueStatus.EstimatedWaitTime <= 10)
                {
                    // Create email content for 10-minute reminder
                    var emailContent = $@"Dear {appointment.Name},

Your appointment is coming up in about 10 minutes.
Queue Number: {appointment.QueueNumber}
Current Position: {queueStatus.PeopleAhead + 1}
Estimated Wait Time: {queueStatus.EstimatedWaitTime} minutes

Please make sure you're ready when called.

Best regards,
Pharmacy Queue System";

                    // Send the email
                    await emailService.SendEmailAsync(appointment.Email,
                        "Your appointment is in 10 minutes", emailContent);

                    // Record the notification in the database
                    context.Notifications.Add(new Notification
                    {
                        AppointmentID = appointment.AppointmentID,
                        Type = "10MinuteReminder",
                        EmailContent = emailContent,
                        EmailSent = true,
                        NotificationTime = DateTime.Now
                    });
                }

                // Check if we need to send a 5-minute reminder
                var fiveMinReminder = await context.Notifications
                    .FirstOrDefaultAsync(n => n.AppointmentID == appointment.AppointmentID &&
                                            n.Type == "5MinuteReminder");

                // Send 5-minute reminder if not sent before and wait time is <= 5 minutes
                if (fiveMinReminder == null && queueStatus.EstimatedWaitTime <= 5)
                {
                    // Create email content for 5-minute reminder
                    var emailContent = $@"Dear {appointment.Name},

Your appointment is coming up in about 5 minutes!
Queue Number: {appointment.QueueNumber}
Current Position: {queueStatus.PeopleAhead + 1}
Estimated Wait Time: {queueStatus.EstimatedWaitTime} minutes

Please be ready to be called soon.

Best regards,
Pharmacy Queue System";

                    // Send the email
                    await emailService.SendEmailAsync(appointment.Email,
                        "Your appointment is in 5 minutes!", emailContent);

                    // Record the notification in the database
                    context.Notifications.Add(new Notification
                    {
                        AppointmentID = appointment.AppointmentID,
                        Type = "5MinuteReminder",
                        EmailContent = emailContent,
                        EmailSent = true,
                        NotificationTime = DateTime.Now
                    });
                }
            }

            // Save all changes to the database
            await context.SaveChangesAsync();
        }
    }
}