using Microsoft.AspNetCore.Components;
using Client.Enums;

namespace Client.Components.Schedule;

public partial class SubjectEntry : ComponentBase
{
    /// <summary>
    /// Subject type - when set, automatically uses predefined color and icon
    /// </summary>
    [Parameter]
    public SubjectEnums? SubjectType { get; set; }

    /// <summary>
    /// Title/name of the subject
    /// </summary>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Background color override (CSS color value). If not set, uses SubjectType color.
    /// </summary>
    [Parameter]
    public string? ColorOverride { get; set; }

    /// <summary>
    /// Icon class override (Bootstrap icon). If not set, uses SubjectType icon.
    /// </summary>
    [Parameter]
    public string? IconOverride { get; set; }

    /// <summary>
    /// Whether to show the icon
    /// </summary>
    [Parameter]
    public bool ShowIcon { get; set; } = true;

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

    // Computed properties
    private string ThemeClass => DarkMode ? "dark-mode" : "light-mode";
    private string EntryTypeClass => SubjectType == SubjectEnums.BreakTime ? "break-time" : "";
    private bool ShowTime => ShowTimeOverride ?? (HeightPx ?? 60) > 40;
    private string Color => ColorOverride ?? GetSubjectColor(SubjectType);
    private string Icon => IconOverride ?? GetSubjectIcon(SubjectType);

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

    #region Subject Colors

    /// <summary>
    /// Predefined colors for each subject type
    /// </summary>
    private static readonly Dictionary<SubjectEnums, string> SubjectColors = new()
    {
        { SubjectEnums.Maths, "#667eea" },         // Purple/Indigo
        { SubjectEnums.English, "#f59e0b" },       // Amber
        { SubjectEnums.Science, "#10b981" },       // Emerald
        { SubjectEnums.History, "#8b5cf6" },       // Violet
        { SubjectEnums.Geography, "#84cc16" },     // Lime
        { SubjectEnums.Physics, "#06b6d4" },       // Cyan
        { SubjectEnums.Chemistry, "#f97316" },     // Orange
        { SubjectEnums.Biology, "#22c55e" },       // Green
        { SubjectEnums.ComputerScience, "#3b82f6" }, // Blue
        { SubjectEnums.Art, "#ec4899" },           // Pink
        { SubjectEnums.Music, "#a855f7" },         // Purple
        { SubjectEnums.PE, "#ef4444" },            // Red
        { SubjectEnums.BreakTime, "rgba(128, 128, 128, 0.3)" }, // Gray semi-transparent
        { SubjectEnums.Other, "#64748b" }          // Slate
    };

    private static string GetSubjectColor(SubjectEnums? subjectType)
    {
        if (subjectType.HasValue && SubjectColors.TryGetValue(subjectType.Value, out var color))
        {
            return color;
        }
        return "#667eea"; // Default purple
    }

    #endregion

    #region Subject Icons

    /// <summary>
    /// Predefined Bootstrap icons for each subject type
    /// </summary>
    private static readonly Dictionary<SubjectEnums, string> SubjectIcons = new()
    {
        { SubjectEnums.Maths, "bi bi-calculator" },
        { SubjectEnums.English, "bi bi-book" },
        { SubjectEnums.Science, "bi bi-radioactive" },
        { SubjectEnums.History, "bi bi-hourglass-split" },
        { SubjectEnums.Geography, "bi bi-globe-americas" },
        { SubjectEnums.Physics, "bi bi-lightning-charge" },
        { SubjectEnums.Chemistry, "bi bi-droplet-half" },
        { SubjectEnums.Biology, "bi bi-tree" },
        { SubjectEnums.ComputerScience, "bi bi-laptop" },
        { SubjectEnums.Art, "bi bi-palette" },
        { SubjectEnums.Music, "bi bi-music-note-beamed" },
        { SubjectEnums.PE, "bi bi-dribbble" },
        { SubjectEnums.BreakTime, "bi bi-cup-hot" },
        { SubjectEnums.Other, "bi bi-bookmark" }
    };

    private static string GetSubjectIcon(SubjectEnums? subjectType)
    {
        if (subjectType.HasValue && SubjectIcons.TryGetValue(subjectType.Value, out var icon))
        {
            return icon;
        }
        return "bi bi-bookmark"; // Default icon
    }

    #endregion
}
