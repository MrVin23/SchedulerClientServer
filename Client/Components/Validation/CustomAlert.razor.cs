using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Components.Validation
{
    public partial class CustomAlert : ComponentBase
    {
        /// <summary>
        /// The message to display in the alert. Ignored if ChildContent is provided.
        /// </summary>
        [Parameter]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The severity of the alert (Normal, Info, Success, Warning, Error).
        /// </summary>
        [Parameter]
        public Severity Severity { get; set; } = Severity.Info;

        /// <summary>
        /// The variant style of the alert (Text, Filled, Outlined).
        /// </summary>
        [Parameter]
        public Variant Variant { get; set; } = Variant.Filled;

        /// <summary>
        /// The horizontal alignment of the alert content.
        /// </summary>
        [Parameter]
        public HorizontalAlignment ContentAlignment { get; set; } = HorizontalAlignment.Left;

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
        /// Uses less vertical padding.
        /// </summary>
        [Parameter]
        public bool Dense { get; set; } = false;

        /// <summary>
        /// The elevation (shadow depth) of the alert.
        /// </summary>
        [Parameter]
        public int Elevation { get; set; } = 0;

        /// <summary>
        /// If true, disables rounded corners.
        /// </summary>
        [Parameter]
        public bool Square { get; set; } = false;

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

        private async Task HandleClose()
        {
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
    }
}

