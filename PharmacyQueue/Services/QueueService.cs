using PharmacyQueue.Data;
using PharmacyQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace PharmacyQueue.Services
{
    /// Service responsible for managing the pharmacy queue and calculating wait times.
    /// Provides functionality to check queue status and estimate wait times for customers.
    public class QueueService
    {
        // Database context for accessing appointment and queue data
        private readonly PharmacyDbContext _context;

        /// <summary>
        /// Initializes a new instance of the QueueService.
        /// </summary>
        /// <param name="context">Database context for accessing queue data</param>
        public QueueService(PharmacyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the current status of a queue number, including wait time and position.
        /// </summary>
        /// <param name="queueNumber">The queue number to check (format: PHAR-YYYYMMDD-XXX)</param>
        /// <returns>A QueueStatus object containing queue information</returns>
        public async Task<QueueStatus> GetQueueStatus(string queueNumber)
        {
            // Find the appointment associated with the queue number
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.QueueNumber == queueNumber);

            // Return invalid status if queue number doesn't exist
            if (appointment == null)
                return QueueStatus.Invalid(queueNumber);

            // If appointment is completed or cancelled, return status with no wait time
            if ((AppointmentStatus)appointment.Status != AppointmentStatus.Waiting)
            {
                return new QueueStatus(
                    queueNumber,
                    0,
                    0,
                    (AppointmentStatus)appointment.Status,
                    true,
                    appointment.CreatedTime
                );
            }

            // Get queue settings for average wait time calculation
            var queueSettings = await _context.QueueSettings.FirstOrDefaultAsync();
            int averageWaitTime = queueSettings?.AverageWaitTime ?? 15; // Default 15 minutes if not set

            // Extract the date part from the queue number
            string currentDate = queueNumber.Split('-')[1];

            // Count people ahead in the queue for the same day who are still waiting
            var peopleAhead = await _context.Appointments
                .Where(a => a.QueueNumber.StartsWith($"PHAR-{currentDate}") // Same day
                    && (AppointmentStatus)a.Status == AppointmentStatus.Waiting // Still waiting
                    && a.QueueNumber.CompareTo(queueNumber) < 0) // Lower queue number
                .CountAsync();

            // Calculate total wait time based on number of people ahead
            var originalTotalWaitTime = (peopleAhead + 1) * averageWaitTime;

            // Calculate time elapsed since appointment creation
            var timeElapsed = DateTime.Now - appointment.CreatedTime;
            var minutesElapsed = (int)timeElapsed.TotalMinutes;

            // Calculate remaining wait time, ensuring it's not negative
            var estimatedWaitTime = Math.Max(0, originalTotalWaitTime - minutesElapsed);

            // Set wait time to 0 if no one is ahead and original wait time has passed
            if (peopleAhead == 0 && estimatedWaitTime <= 0)
            {
                estimatedWaitTime = 0;
            }

            // Return the current queue status
            return new QueueStatus(
                queueNumber,
                peopleAhead,
                estimatedWaitTime,
                (AppointmentStatus)appointment.Status,
                true,
                appointment.CreatedTime
            );
        }
    }

    /// <summary>
    /// Represents the current status of a queue number in the pharmacy system.
    /// Contains information about position, wait time, and appointment status.
    /// </summary>
    public class QueueStatus
    {
        // Whether the queue number is valid
        public bool IsValid { get; init; }
        // The queue number being checked
        public string QueueNumber { get; init; }
        // Number of people ahead in the queue
        public int PeopleAhead { get; init; }
        // Estimated wait time in minutes
        public int EstimatedWaitTime { get; init; }
        // Current status of the appointment
        public AppointmentStatus Status { get; init; }
        // When this status was last updated
        public DateTime LastUpdated { get; init; }
        // When the appointment was created
        public DateTime AppointmentCreatedTime { get; init; }

        /// <summary>
        /// Creates a new QueueStatus with the specified parameters.
        /// </summary>
        public QueueStatus(string queueNumber, int peopleAhead, int estimatedWaitTime,
            AppointmentStatus status, bool isValid, DateTime createdTime)
        {
            QueueNumber = queueNumber;
            PeopleAhead = peopleAhead;
            EstimatedWaitTime = estimatedWaitTime;
            Status = status;
            IsValid = isValid;
            LastUpdated = DateTime.Now;
            AppointmentCreatedTime = createdTime;
        }

        /// <summary>
        /// Creates an invalid QueueStatus for non-existent queue numbers.
        /// </summary>
        public static QueueStatus Invalid(string queueNumber) =>
            new QueueStatus(queueNumber, 0, 0, AppointmentStatus.Waiting, false, DateTime.Now);
    }
}