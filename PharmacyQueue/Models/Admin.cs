using System.ComponentModel.DataAnnotations;

namespace PharmacyQueue.Models
{
    /// This is for staff member with administrative access to the system.
    /// This model handles authentication and authorization for pharmacy staff.
    public class Admin
    {
        // Unique identifier for the admin user
        [Key]
        public int AdminID { get; set; }

        // Admin's full name - required field with max length of 100 characters
        [Required, StringLength(100)]
        public string Name { get; set; }

        // Admin's email address - required field with email validation
        [Required, EmailAddress]
        public string Email { get; set; }

        // Admin's password - required field, should be hashed before storage
        // Max length of 256 characters to accommodate hash
        [Required, StringLength(256)]
        public string Password { get; set; } // Store hashed password

        // Admin's role in the system - either "Super" or "Staff"
        // Defaults to "Staff" for new accounts
        [Required]
        public string Role { get; set; } = "Staff"; // Super or Staff

        // Timestamp of the admin's last successful login
        // Null if they haven't logged in yet
        public DateTime? LastLogin { get; set; }

        // Whether the admin account is currently active
        // Can be used to temporarily disable access
        public bool IsActive { get; set; } = true;

        // When the admin account was created
        // Automatically set to current time when created
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}