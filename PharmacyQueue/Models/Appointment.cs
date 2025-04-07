using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyQueue.Models
{
    /// Represents a pharmacy appointment in the queue system.
    /// This model stores all information about a customer's visit to the pharmacy.
    public class Appointment
    {
        // Unique identifier for the appointment
        [Key]
        public int AppointmentID { get; set; }

        // Customer's full name - required field with max length of 100 characters
        [Required(ErrorMessage = "Please enter your name")]
        [StringLength(100)]
        public string Name { get; set; }

        // Customer's email address - required field with email validation
        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100)]
        public string Email { get; set; }

        // Customer's phone number - required field with phone number validation
        [Required(ErrorMessage = "Please enter your phone number")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20)]
        public string Phone { get; set; }

        // Purpose of the visit - required field with max length of 500 characters
        [Required(ErrorMessage = "Please select a purpose for your visit")]
        [StringLength(500)]
        public string Purpose { get; set; }

        // Optional additional notes about the appointment
        [StringLength(500)]
        public string? AdditionalNotes { get; set; }

        // Queue number assigned to the customer (optional)
        [StringLength(20)]
        public string? QueueNumber { get; set; } = null;

        // Current status of the appointment:
        // 0 = Waiting in queue
        // 1 = Currently being served
        // 2 = Completed
        // 4 = Cancelled
        public int Status { get; set; } = 0;

        // When the appointment was created (automatically set to current time)
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        // Last time the appointment was updated (optional)
        public DateTime? LastUpdated { get; set; }
    }
}