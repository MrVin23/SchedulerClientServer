using Microsoft.AspNetCore.Components;

namespace Client.Components.Schedule;

public partial class SubjectEntry : ComponentBase
{
    /// <summary>
    /// Title/name of the subject
    /// </summary>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Background color (CSS color value)
    /// </summary>
    [Parameter]
    public string Color { get; set; } = "#667eea";

    /// <summary>
    /// Start time of the entry
    /// </summary>
    [Parameter]
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// End time of the entry
    /// </summary>
    [Parameter]
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Top position in pixels (for absolute positioning in grid)
    /// </summary>
    [Parameter]
    public double? TopPx { get; set; }

    /// <summary>
    /// Height in pixels
    /// </summary>
    [Parameter]
    public double? HeightPx { get; set; }

    /// <summary>
    /// Whether to use dark mode styling
    /// </summary>
    [Parameter]
    public bool DarkMode { get; set; } = false;

    /// <summary>
    /// Whether to show the time range (auto-hides on small entries if not specified)
    /// </summary>
    [Parameter]
    public bool? ShowTimeOverride { get; set; }

    private string ThemeClass => DarkMode ? "dark-mode" : "light-mode";

    private bool ShowTime => ShowTimeOverride ?? (HeightPx ?? 60) > 40;

    private string PositionStyle
    {
        get
        {
            var styles = new List<string>();
            
            if (TopPx.HasValue)
            {
                styles.Add($"top: {TopPx.Value}px");
            }
            
            if (HeightPx.HasValue)
            {
                styles.Add($"height: {HeightPx.Value}px");
            }
            
            return styles.Count > 0 ? string.Join("; ", styles) + ";" : string.Empty;
        }
    }
}
