using Microsoft.AspNetCore.Components;

namespace Client.Components.Schedule;

public partial class WeekGrid : ComponentBase
{
    /// <summary>
    /// Schedule entries to display in the grid
    /// </summary>
    [Parameter]
    public List<WeekGridEntry> Entries { get; set; } = new();

    /// <summary>
    /// The week to display (any date within the desired week)
    /// </summary>
    [Parameter]
    public DateTime WeekOf { get; set; } = DateTime.Today;

    /// <summary>
    /// Start time of the schedule day
    /// </summary>
    [Parameter]
    public TimeSpan DayStartTime { get; set; } = new TimeSpan(7, 0, 0);

    /// <summary>
    /// End time of the schedule day
    /// </summary>
    [Parameter]
    public TimeSpan DayEndTime { get; set; } = new TimeSpan(17, 0, 0);

    /// <summary>
    /// Time interval between slots in minutes
    /// </summary>
    [Parameter]
    public int SlotIntervalMinutes { get; set; } = 30;

    /// <summary>
    /// Height of each time slot in pixels
    /// </summary>
    [Parameter]
    public int SlotHeightPx { get; set; } = 60;

    /// <summary>
    /// Whether to use dark mode (false = light mode)
    /// </summary>
    [Parameter]
    public bool DarkMode { get; set; } = false;

    /// <summary>
    /// First day of the week
    /// </summary>
    [Parameter]
    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

    /// <summary>
    /// Number of days to display (5 for weekdays, 7 for full week)
    /// </summary>
    [Parameter]
    public int DaysToShow { get; set; } = 5;

    private string ThemeClass => DarkMode ? "dark-mode" : "light-mode";

    private IEnumerable<TimeSpan> TimeSlots
    {
        get
        {
            var slots = new List<TimeSpan>();
            var current = DayStartTime;
            while (current < DayEndTime)
            {
                slots.Add(current);
                current = current.Add(TimeSpan.FromMinutes(SlotIntervalMinutes));
            }
            return slots;
        }
    }

    private IEnumerable<DateTime> WeekDays
    {
        get
        {
            var startOfWeek = GetStartOfWeek(WeekOf);
            return Enumerable.Range(0, DaysToShow).Select(i => startOfWeek.AddDays(i));
        }
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - FirstDayOfWeek)) % 7;
        return date.AddDays(-diff).Date;
    }

    private double TotalGridHeight => TimeSlots.Count() * SlotHeightPx;

    private double CalculateTopPosition(TimeSpan time)
    {
        var minutesFromStart = (time - DayStartTime).TotalMinutes;
        var pixelsPerMinute = (double)SlotHeightPx / SlotIntervalMinutes;
        return minutesFromStart * pixelsPerMinute;
    }

    private double CalculateHeight(TimeSpan startTime, TimeSpan endTime)
    {
        var durationMinutes = (endTime - startTime).TotalMinutes;
        var pixelsPerMinute = (double)SlotHeightPx / SlotIntervalMinutes;
        return Math.Max(durationMinutes * pixelsPerMinute, 20); // Minimum height of 20px
    }

    private IEnumerable<WeekGridEntry> GetEntriesForDay(DateTime day)
    {
        return Entries.Where(e => e.DayOfWeek == day.DayOfWeek);
    }
}

/// <summary>
/// Represents a schedule entry in the week grid
/// </summary>
public class WeekGridEntry
{
    /// <summary>
    /// Day of the week for this entry
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Start time of the entry
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// End time of the entry
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Display title for the entry
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Background color for the entry (CSS color value)
    /// </summary>
    public string Color { get; set; } = "#667eea";
}


