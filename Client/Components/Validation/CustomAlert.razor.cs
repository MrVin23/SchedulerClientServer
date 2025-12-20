using Microsoft.AspNetCore.Components;
using Client.Enums;

namespace Client.Components.Validation
{
    public partial class CustomAlert : ComponentBase, IDisposable
    {
        private CancellationTokenSource? _autoCloseCts;

        /// <summary>
        /// The message to display in the alert. Ignored if ChildContent is provided.
        /// </summary>
        [Parameter]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The severity of the alert (Info, Success, Warning, Error).
        /// </summary>
        [Parameter]
        public AlertSeverity Severity { get; set; } = AlertSeverity.Info;

        /// <summary>
        /// The variant style of the alert (Text, Filled, Outlined).
        /// </summary>
        [Parameter]
        public AlertVariant Variant { get; set; } = AlertVariant.Filled;

        /// <summary>
        /// Whether the alert is currently visible.
        /// </summary>
        [Parameter]
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Event callback when visibility changes (e.g., when close button is clicked).
        /// </summary>
        [Parameter]
        public EventCallback<bool> IsVisibleChanged { get; set; }

        /// <summary>
        /// Event callback when the close icon is clicked.
        /// </summary>
        [Parameter]
        public EventCallback OnClose { get; set; }

        /// <summary>
        /// Additional CSS classes to apply to the alert.
        /// </summary>
        [Parameter]
        public string? Class { get; set; }

        /// <summary>
        /// Custom content to display inside the alert. Overrides Message if provided.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// If true, automatically closes the alert after 5 seconds.
        /// </summary>
        [Parameter]
        public bool DelayedClose { get; set; } = false;

        protected override void OnParametersSet()
        {
            // Cancel any existing auto-close timer
            _autoCloseCts?.Cancel();
            _autoCloseCts?.Dispose();
            _autoCloseCts = null;

            // Start new auto-close timer if enabled and visible
            if (DelayedClose && IsVisible)
            {
                _autoCloseCts = new CancellationTokenSource();
                _ = AutoCloseAsync(_autoCloseCts.Token);
            }
        }

        private async Task AutoCloseAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(5000, cancellationToken);
                if (!cancellationToken.IsCancellationRequested && IsVisible)
                {
                    await HandleClose();
                    StateHasChanged();
                }
            }
            catch (TaskCanceledException)
            {
                // Timer was cancelled, ignore
            }
        }

        private async Task HandleClose()
        {
            // Cancel auto-close timer when manually closed
            _autoCloseCts?.Cancel();
            
            IsVisible = false;
            await IsVisibleChanged.InvokeAsync(false);
            await OnClose.InvokeAsync();
        }

        /// <summary>
        /// Shows the alert.
        /// </summary>
        public async Task ShowAsync()
        {
            IsVisible = true;
            await IsVisibleChanged.InvokeAsync(true);
            StateHasChanged();
        }

        /// <summary>
        /// Hides the alert.
        /// </summary>
        public async Task HideAsync()
        {
            IsVisible = false;
            await IsVisibleChanged.InvokeAsync(false);
            StateHasChanged();
        }

        public void Dispose()
        {
            _autoCloseCts?.Cancel();
            _autoCloseCts?.Dispose();
        }
    }
}
