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

    /// <summary>
    /// Weeks of the month this entry occurs (0 = every week, 1-4 = specific weeks)
    /// </summary>
    [Parameter]
    public List<int> WeeksOfMonth { get; set; } = new() { 0 };

    /// <summary>
    /// Current week of the month (1-4) for determining active card
    /// </summary>
    [Parameter]
    public int CurrentWeekOfMonth { get; set; } = GetCurrentWeekOfMonth();

    /// <summary>
    /// Alternate entries for other weeks at the same time slot (for card stack display)
    /// </summary>
    [Parameter]
    public List<AlternateWeekEntry> AlternateWeekEntries { get; set; } = new();

    // Computed properties
    private string ThemeClass => DarkMode ? "dark-mode" : "light-mode";
    private string EntryTypeClass => SubjectType == SubjectEnums.BreakTime ? "break-time" : "";
    private bool ShowTime => ShowTimeOverride ?? (HeightPx ?? 60) > 40;
    private string Color => ColorOverride ?? GetSubjectColor(SubjectType);
    private string Icon => IconOverride ?? GetSubjectIcon(SubjectType);

    // Card stack properties
    private bool HasAlternateWeeks => AlternateWeekEntries.Count > 0;
    
    /// <summary>
    /// Formatted string of weeks for the active entry (e.g., "Week 1, 3")
    /// </summary>
    private string ActiveWeeksDisplay
    {
        get
        {
            if (WeeksOfMonth.Count == 0 || (WeeksOfMonth.Count == 1 && WeeksOfMonth[0] == 0))
            {
                return string.Empty;
            }
            var weeks = WeeksOfMonth.Where(w => w > 0).OrderBy(w => w);
            return $"Week {string.Join(", ", weeks)}";
        }
    }

    private List<StackedCardInfo> InactiveWeekEntries
    {
        get
        {
            // Get all alternate entries that are NOT for the current week
            return AlternateWeekEntries
                .Where(e => !e.WeeksOfMonth.Contains(CurrentWeekOfMonth))
                .Select(e => new StackedCardInfo
                {
                    WeeksDisplay = FormatWeeks(e.WeeksOfMonth),
                    Title = e.Title,
                    Color = e.ColorOverride ?? GetSubjectColor(e.SubjectType)
                })
                .OrderBy(e => e.Title)
                .ToList();
        }
    }

    private static string FormatWeeks(List<int> weeks)
    {
        if (weeks.Count == 0) return string.Empty;
        var validWeeks = weeks.Where(w => w > 0).OrderBy(w => w);
        return $"Week {string.Join(", ", validWeeks)}";
    }

    private string GetAltColor(string baseColor)
    {
        // Return a slightly darker/muted version for stacked cards
        return $"color-mix(in srgb, {baseColor} 70%, #000000 30%)";
    }

    private static int GetCurrentWeekOfMonth()
    {
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        // Adjust for Monday as first day of week
        if (firstDayOfWeek == 0) firstDayOfWeek = 7;
        var dayOfMonth = today.Day;
        return (dayOfMonth + firstDayOfWeek - 2) / 7 + 1;
    }

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

/// <summary>
/// Represents an alternate week entry for the card stack
/// </summary>
public class AlternateWeekEntry
{
    public string Title { get; set; } = string.Empty;
    public SubjectEnums? SubjectType { get; set; }
    public string? ColorOverride { get; set; }
    public List<int> WeeksOfMonth { get; set; } = new();
}

/// <summary>
/// Internal class for rendering stacked card info
/// </summary>
internal class StackedCardInfo
{
    public string WeeksDisplay { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
