using Microsoft.AspNetCore.Components;

namespace Client.Components.CustomButton;

public partial class CustomButton : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Button text content
    /// </summary>
    [Parameter]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Child content to render inside the button
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Style type of the button component
    /// </summary>
    [Parameter]
    public CustomButtonType ButtonType { get; set; } = CustomButtonType.Primary;

    /// <summary>
    /// Bootstrap icon class (e.g., "bi bi-check")
    /// </summary>
    [Parameter]
    public string IconClass { get; set; } = string.Empty;

    /// <summary>
    /// Position of the icon relative to text
    /// </summary>
    [Parameter]
    public ButtonIconPosition IconPosition { get; set; } = ButtonIconPosition.Left;

    /// <summary>
    /// Width of the button
    /// </summary>
    [Parameter]
    public string Width { get; set; } = "auto";

    /// <summary>
    /// Height of the button
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "auto";

    /// <summary>
    /// Whether the button is disabled
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// Callback when button is clicked
    /// </summary>
    [Parameter]
    public EventCallback<Microsoft.AspNetCore.Components.Web.MouseEventArgs> OnClick { get; set; }

    private async Task HandleClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
    {
        if (!Disabled)
        {
            await OnClick.InvokeAsync(args);
        }
    }

    private string ButtonCssClass => ButtonType switch
    {
        CustomButtonType.Pager => "custom-button pager",
        CustomButtonType.Secondary => "custom-button secondary",
        CustomButtonType.Danger => "custom-button danger",
        CustomButtonType.Ghost => "custom-button ghost",
        _ => "custom-button primary"
    };
}

public enum CustomButtonType
{
    /// <summary>
    /// Primary style - main action button
    /// </summary>
    Primary,

    /// <summary>
    /// Secondary style - alternative action button
    /// </summary>
    Secondary,

    /// <summary>
    /// Pager style - compact button for pager components
    /// </summary>
    Pager,

    /// <summary>
    /// Danger style - destructive action button
    /// </summary>
    Danger,

    /// <summary>
    /// Ghost style - minimal/transparent button
    /// </summary>
    Ghost
}

public enum ButtonIconPosition
{
    Left,
    Right
}

