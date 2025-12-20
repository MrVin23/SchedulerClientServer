using Client.Enums;
using Client.Interfaces;

namespace Client.Services
{
    /// <summary>
    /// Service for displaying global alerts throughout the application.
    /// Inject this service to show validation errors, server response errors, warnings, and informational alerts.
    /// </summary>
    public class AlertService : IAlertService
    {
        public event Action<AlertMessage>? OnAlert;
        public event Action? OnClear;

        public void ShowAlert(string message, AlertSeverity severity = AlertSeverity.Info, AlertVariant variant = AlertVariant.Filled)
        {
            OnAlert?.Invoke(new AlertMessage
            {
                Message = message,
                Severity = severity,
                Variant = variant
            });
        }

        public void ShowError(string message)
        {
            ShowAlert(message, AlertSeverity.Error, AlertVariant.Filled);
        }

        public void ShowWarning(string message)
        {
            ShowAlert(message, AlertSeverity.Warning, AlertVariant.Filled);
        }

        public void ShowSuccess(string message)
        {
            ShowAlert(message, AlertSeverity.Success, AlertVariant.Filled);
        }

        public void ShowInfo(string message)
        {
            ShowAlert(message, AlertSeverity.Info, AlertVariant.Filled);
        }

        public void ClearAlert()
        {
            OnClear?.Invoke();
        }
    }
}
