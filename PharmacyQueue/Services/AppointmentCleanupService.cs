using Microsoft.EntityFrameworkCore;
using PharmacyQueue.Data;
using PharmacyQueue.Models;

namespace PharmacyQueue.Services
{
    // Background service that automatically cleans up old appointments from the database.
    // Runs continuously to remove appointments older than 6 months to maintain database performance.
    public class AppointmentCleanupService : BackgroundService
    {
        // Service provider for dependency injection
        private readonly IServiceProvider _serviceProvider;
        // Logger for tracking cleanup operations and errors
        private readonly ILogger<AppointmentCleanupService> _logger;

        // Initializes a new instance of the AppointmentCleanupService.
        // serviceProvider: Service provider for dependency injection
        // logger: Logger for tracking operations
        public AppointmentCleanupService(IServiceProvider serviceProvider, ILogger<AppointmentCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        // Executes the background service continuously.
        // Performs cleanup of old appointments every 24 hours.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Appointment Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting cleanup cycle at: {time}", DateTimeOffset.Now);

                    // Create a new scope for database operations
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var db = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();

                    // Calculate the cutoff date (6 months ago)
                    var sixMonthsAgo = DateTime.Now.AddMonths(-6);

                    // Find all appointments older than 6 months
                    var oldAppointments = await db.Appointments
                        .Where(a => a.CreatedTime < sixMonthsAgo)
                        .ToListAsync(stoppingToken);

                    // If there are old appointments, remove them
                    if (oldAppointments.Any())
                    {
                        _logger.LogInformation("Found {count} appointments to delete", oldAppointments.Count);
                        db.Appointments.RemoveRange(oldAppointments);
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Successfully deleted {count} appointments older than 6 months", oldAppointments.Count);
                    }
                    else
                    {
                        _logger.LogInformation("No appointments found to clean up");
                    }

                    // Wait for 24 hours before next cleanup cycle
                    try
                    {
                        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Cleanup service is shutting down");
                        break;
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Log any errors that occur during cleanup
                    _logger.LogError(ex, "Error occurred while cleaning up old appointments");

                    // Wait for 1 hour before retrying after an error
                    try
                    {
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Cleanup service is shutting down");
                        break;
                    }
                }
            }

            _logger.LogInformation("Appointment Cleanup Service is stopping.");
        }
    }
}