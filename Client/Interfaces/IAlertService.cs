using Client.Enums;

namespace Client.Interfaces
{
    /// <summary>
    /// Service for displaying global alerts throughout the application.
    /// Use this service to show validation errors, server response errors, warnings, and informational alerts.
    /// </summary>
    public interface IAlertService
    {
        /// <summary>
        /// Event raised when an alert should be displayed.
        /// </summary>
        event Action<AlertMessage>? OnAlert;

        /// <summary>
        /// Event raised when the alert should be cleared/hidden.
        /// </summary>
        event Action? OnClear;

        /// <summary>
        /// Shows an alert with the specified message and severity.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="severity">The severity level of the alert.</param>
        /// <param name="variant">The visual variant of the alert.</param>
        void ShowAlert(string message, AlertSeverity severity = AlertSeverity.Info, AlertVariant variant = AlertVariant.Filled);

        /// <summary>
        /// Shows an error alert. Use for server errors, API failures, and critical issues.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        void ShowError(string message);

        /// <summary>
        /// Shows a warning alert. Use for validation warnings and non-critical issues.
        /// </summary>
        /// <param name="message">The warning message to display.</param>
        void ShowWarning(string message);

        /// <summary>
        /// Shows a success alert. Use for successful operations.
        /// </summary>
        /// <param name="message">The success message to display.</param>
        void ShowSuccess(string message);

        /// <summary>
        /// Shows an info alert. Use for informational messages.
        /// </summary>
        /// <param name="message">The info message to display.</param>
        void ShowInfo(string message);

        /// <summary>
        /// Clears/hides the current alert.
        /// </summary>
        void ClearAlert();
    }

    /// <summary>
    /// Represents an alert message with its properties.
    /// </summary>
    public class AlertMessage
    {
        public string Message { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; } = AlertSeverity.Info;
        public AlertVariant Variant { get; set; } = AlertVariant.Filled;
    }
}
