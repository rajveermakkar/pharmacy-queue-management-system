using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyQueue.Models
{
    /// Represents a notification in the system, typically for appointment reminders or status updates.
    /// This model tracks email notifications sent to customers about their appointments.
    public class Notification
    {
        // Unique identifier for the notification
        [Key]
        public int NotificationID { get; set; }

        // Reference to the associated appointment
        public int AppointmentID { get; set; }

        // Navigation property to the associated appointment
        // Virtual for lazy loading
        [ForeignKey("AppointmentID")]
        public virtual Appointment? Appointment { get; set; }

        // Type of notification (e.g., "Reminder", "Status Update", "Cancellation")
        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        // Whether the email has been successfully sent
        // Defaults to false until email is sent
        public bool EmailSent { get; set; } = false;

        // The actual content of the email to be sent
        [Required]
        [StringLength(500)]
        public string EmailContent { get; set; }

        // When the notification was created
        // Automatically set to current time when created
        public DateTime NotificationTime { get; set; } = DateTime.Now;
    }
}