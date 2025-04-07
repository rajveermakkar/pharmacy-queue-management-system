namespace PharmacyQueue.Models;

/// View model used for displaying error pages in the application.
/// This model provides information about errors that occur during request processing.
public class ErrorViewModel
{
    // Unique identifier for the request that caused the error
    // Used for tracking and debugging purposes
    public string? RequestId { get; set; }

    // Computed property that determines if the RequestId should be displayed
    // Only shows the ID if it's not null or empty
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
