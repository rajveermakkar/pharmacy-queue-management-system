namespace PharmacyQueue.Models
{
    /// Defines the possible states of an appointment in the pharmacy queue system.
    /// Each status represents a different stage in the appointment lifecycle.
    public enum AppointmentStatus
    {
        // Customer has checked in and is waiting to be served
        Waiting = 0,

        // Customer is currently being served by pharmacy staff
        InProgress = 1,

        // Customer has been served and the appointment is finished
        Completed = 2,

        // Appointment was cancelled by either staff or customer
        Cancelled = 3,

        // Customer did not show up for their appointment
        NoShow = 4
    }
}