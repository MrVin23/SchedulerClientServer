using Microsoft.AspNetCore.Components;

namespace Client.Components.Pagination;

public partial class PagingButton : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Direction of the paging button
    /// </summary>
    [Parameter]
    public PagingDirection Direction { get; set; } = PagingDirection.Next;

    /// <summary>
    /// Custom icon class (overrides Direction-based icon)
    /// </summary>
    [Parameter]
    public string? CustomIconClass { get; set; }

    /// <summary>
    /// Custom title/tooltip text (overrides Direction-based title)
    /// </summary>
    [Parameter]
    public string? CustomTitle { get; set; }

    /// <summary>
    /// Style type of the paging button
    /// </summary>
    [Parameter]
    public PagingButtonType ButtonType { get; set; } = PagingButtonType.Default;

    /// <summary>
    /// Whether the button is disabled
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// Position of the tooltip relative to the button
    /// </summary>
    [Parameter]
    public TooltipPosition TooltipPosition { get; set; } = TooltipPosition.Bottom;

    /// <summary>
    /// Callback when button is clicked
    /// </summary>
    [Parameter]
    public EventCallback<Microsoft.AspNetCore.Components.Web.MouseEventArgs> OnClick { get; set; }

    private string IconClass => CustomIconClass ?? Direction switch
    {
        PagingDirection.Previous => "bi bi-chevron-left",
        PagingDirection.Next => "bi bi-chevron-right",
        PagingDirection.First => "bi bi-chevron-double-left",
        PagingDirection.Last => "bi bi-chevron-double-right",
        _ => "bi bi-chevron-right"
    };

    private string Title => CustomTitle ?? Direction switch
    {
        PagingDirection.Previous => "Previous",
        PagingDirection.Next => "Next",
        PagingDirection.First => "First",
        PagingDirection.Last => "Last",
        _ => string.Empty
    };

    private string ButtonCssClass => ButtonType switch
    {
        PagingButtonType.Light => "paging-button light",
        PagingButtonType.Solid => "paging-button solid",
        _ => "paging-button default"
    };

    private string TooltipPositionClass => TooltipPosition switch
    {
        TooltipPosition.Top => "tooltip-top",
        TooltipPosition.Left => "tooltip-left",
        TooltipPosition.Right => "tooltip-right",
        _ => "tooltip-bottom"
    };

    private async Task HandleClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
    {
        if (!Disabled)
        {
            await OnClick.InvokeAsync(args);
        }
    }
}

public enum PagingDirection
{
    /// <summary>
    /// Previous page/item
    /// </summary>
    Previous,

    /// <summary>
    /// Next page/item
    /// </summary>
    Next,

    /// <summary>
    /// First page/item
    /// </summary>
    First,

    /// <summary>
    /// Last page/item
    /// </summary>
    Last
}

public enum PagingButtonType
{
    /// <summary>
    /// Default style - semi-transparent on gradient backgrounds
    /// </summary>
    Default,

    /// <summary>
    /// Light style - lighter background for dark themes
    /// </summary>
    Light,

    /// <summary>
    /// Solid style - opaque background
    /// </summary>
    Solid
}

public enum TooltipPosition
{
    /// <summary>
    /// Tooltip appears above the button
    /// </summary>
    Top,

    /// <summary>
    /// Tooltip appears below the button
    /// </summary>
    Bottom,

    /// <summary>
    /// Tooltip appears to the left of the button
    /// </summary>
    Left,

    /// <summary>
    /// Tooltip appears to the right of the button
    /// </summary>
    Right
}

