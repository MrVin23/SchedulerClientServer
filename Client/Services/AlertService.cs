using Client.Interfaces;
using MudBlazor;

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

        public void ShowAlert(string message, Severity severity = Severity.Info, Variant variant = Variant.Filled)
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
            ShowAlert(message, Severity.Error, Variant.Filled);
        }

        public void ShowWarning(string message)
        {
            ShowAlert(message, Severity.Warning, Variant.Filled);
        }

        public void ShowSuccess(string message)
        {
            ShowAlert(message, Severity.Success, Variant.Filled);
        }

        public void ShowInfo(string message)
        {
            ShowAlert(message, Severity.Info, Variant.Filled);
        }

        public void ClearAlert()
        {
            OnClear?.Invoke();
        }
    }
}

