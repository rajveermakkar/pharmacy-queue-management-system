using System.ComponentModel.DataAnnotations;

namespace PharmacyQueue.Models
{
    // Represents the configuration settings for the pharmacy queue system.
    // This model controls various operational parameters like working hours and booking limits.
    public class QueueSetting
    {
        // Unique identifier for the settings record
        [Key]
        public int SettingID { get; set; }

        // Average time (in minutes) it takes to serve one customer
        // Used for estimating wait times and queue managements
        [Required]
        [Range(1, 60)]
        public int AverageWaitTime { get; set; } = 15;

        // Maximum number of appointments that can be booked per day
        // Helps prevent overbooking and manage pharmacy capacity
        [Required]
        [Range(1, 100)]
        public int MaxDailyBookings { get; set; } = 50;

        // Start time of pharmacy working hours
        // Defaults to 9:00 AM
        [Required]
        public TimeOnly WorkingHoursStart { get; set; } = new TimeOnly(9, 0); // 9:00 AM

        // End time of pharmacy working hours
        // Defaults to 5:00 PM
        [Required]
        public TimeOnly WorkingHoursEnd { get; set; } = new TimeOnly(17, 0); // 5:00 PM

        // Indicates if the current day is a holiday
        // Used to prevent bookings on holidays
        public bool IsHoliday { get; set; }

        // When the settings were last updated
        // Automatically set to current time when modified
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}