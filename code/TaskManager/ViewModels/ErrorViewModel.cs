namespace TaskManagerWebsite.ViewModels
{
    /// <summary>
    /// Represents the model used for displaying error information.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the current request, if available.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Determines whether the request ID should be displayed.
        /// Returns true if the request ID is not null or empty.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);
    }
}
