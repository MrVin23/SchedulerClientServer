using Microsoft.AspNetCore.Components;

namespace Client.Components.Pagination;

public partial class YearRoomPager : ComponentBase
{
    // Info to populate the dropdowns
    public List<string> MockYears = new List<string> { "Year 1", "Year 2", "Year 3", "Year 4", "Year 5", "Year 6" };
    public List<string> MockRooms = new List<string> { "Room 1", "Room 2", "Room 3", "Room 4", "Room 5", "Room 6" };
    
    // Selected values
    public string? SelectedYear { get; set; } = "Year 1";
    public string? SelectedRoom { get; set; } = "Room 1";
    
    // Mock data for details section
    public string TeacherName = "John Doe";
    public DateTime LastUpdated = DateTime.Now;
    public int TotalRooms = 6;

    // Computed property for current week of month
    private int CurrentWeekOfMonth
    {
        get
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            // Adjust for Monday as first day of week (0 = Sunday becomes 7)
            if (firstDayOfWeek == 0) firstDayOfWeek = 7;
            var dayOfMonth = today.Day;
            return (dayOfMonth + firstDayOfWeek - 2) / 7 + 1;
        }
    }

    // Event Callbacks (for parent component integration)
    [Parameter] public EventCallback OnPrevious { get; set; }
    [Parameter] public EventCallback OnNext { get; set; }

    // Placeholder methods (no logic, design testing only)
    private void OnPreviousClicked()
    {
        // TODO: Implement previous paging logic
    }

    private void OnNextClicked()
    {
        // TODO: Implement next paging logic
    }
}

