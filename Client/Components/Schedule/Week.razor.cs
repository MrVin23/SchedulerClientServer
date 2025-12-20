using Microsoft.AspNetCore.Components;

namespace Client.Components.Schedule;

public partial class Week : ComponentBase
{
    /// <summary>
    /// The classroom ID to display the schedule for.
    /// </summary>
    [Parameter]
    public int ClassroomId { get; set; } = 1;

    /// <summary>
    /// The currently displayed week's starting date (Monday).
    /// </summary>
    [Parameter]
    public DateTime DisplayWeekStart { get; set; } = GetMondayOfWeek(DateTime.Today);

    /// <summary>
    /// Event callback when the display week changes.
    /// </summary>
    [Parameter]
    public EventCallback<DateTime> DisplayWeekStartChanged { get; set; }

    /// <summary>
    /// Event callback when a schedule entry is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<WeekMockData.ScheduleEntryDto> OnEntryClick { get; set; }

    // Internal state
    private List<WeekMockData.ScheduleEntryDto> ScheduleEntries { get; set; } = new();
    private List<WeekMockData.SubjectDefinition> Subjects { get; set; } = new();
    private WeekMockData.ClassroomDto? Classroom { get; set; }
    private WeekMockData.ScheduleEntryDto? SelectedEntry { get; set; }
    private bool IsLoading { get; set; } = true;

    // Constants for grid positioning
    private const int StartHour = 8;
    private const int EndHour = 15;
    private const double PixelsPerMinute = 1.2;
    private const int HourSlotHeight = 72; // 60 minutes * PixelsPerMinute

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        // Reload data when classroom changes
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        StateHasChanged();

        try
        {
            Subjects = WeekMockData.GetSubjects();
            Classroom = WeekMockData.GetClassroom();
            ScheduleEntries = await WeekMockData.GetWeekScheduleAsync(ClassroomId);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private static DateTime GetMondayOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private string GetWeekTitle()
    {
        var monday = DisplayWeekStart;
        var friday = monday.AddDays(4);
        
        if (monday.Month == friday.Month)
        {
            return monday.ToString("MMMM yyyy");
        }
        
        return $"{monday:MMM} - {friday:MMM yyyy}";
    }

    private string GetWeekDateRange()
    {
        var monday = DisplayWeekStart;
        var friday = monday.AddDays(4);
        return $"{monday:MMM d} - {friday:MMM d}";
    }

    private IEnumerable<DateTime> GetWeekDays()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return DisplayWeekStart.AddDays(i);
        }
    }

    private IEnumerable<TimeSpan> GetHourSlots()
    {
        for (int hour = StartHour; hour <= EndHour; hour++)
        {
            yield return new TimeSpan(hour, 0, 0);
        }
    }

    private IEnumerable<WeekMockData.ScheduleEntryDto> GetEntriesForDay(DateTime day)
    {
        return ScheduleEntries
            .Where(e => e.Day == day.DayOfWeek)
            .OrderBy(e => e.StartTime);
    }

    private double CalculateTopPosition(TimeSpan time)
    {
        var minutesFromStart = (time - new TimeSpan(StartHour, 0, 0)).TotalMinutes;
        return Math.Max(0, minutesFromStart * PixelsPerMinute);
    }

    private double CalculateHeight(int durationMinutes)
    {
        return Math.Max(40, durationMinutes * PixelsPerMinute - 2); // -2 for gap
    }

    private string GetSubjectIcon(string iconName)
    {
        // Map to Bootstrap Icons
        return iconName switch
        {
            "Calculate" => "bi-calculator",
            "MenuBook" => "bi-book",
            "Science" => "bi-mortarboard",
            "HistoryEdu" => "bi-clock-history",
            "Public" => "bi-globe",
            "Palette" => "bi-palette",
            "MusicNote" => "bi-music-note-beamed",
            "FitnessCenter" => "bi-trophy",
            _ => "bi-book"
        };
    }

    private async Task NavigateToPreviousWeek()
    {
        DisplayWeekStart = DisplayWeekStart.AddDays(-7);
        await DisplayWeekStartChanged.InvokeAsync(DisplayWeekStart);
    }

    private async Task NavigateToNextWeek()
    {
        DisplayWeekStart = DisplayWeekStart.AddDays(7);
        await DisplayWeekStartChanged.InvokeAsync(DisplayWeekStart);
    }

    private async Task GoToCurrentWeek()
    {
        DisplayWeekStart = GetMondayOfWeek(DateTime.Today);
        await DisplayWeekStartChanged.InvokeAsync(DisplayWeekStart);
    }

    private async Task HandleEntryClick(WeekMockData.ScheduleEntryDto entry)
    {
        SelectedEntry = entry;
        await OnEntryClick.InvokeAsync(entry);
    }

    private void CloseDetailPanel()
    {
        SelectedEntry = null;
    }
}
