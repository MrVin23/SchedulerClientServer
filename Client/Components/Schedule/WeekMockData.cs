namespace Client.Components.Schedule;

/// <summary>
/// Mock data provider for the Week schedule view.
/// Simulates an API response with schedule entries for a classroom.
/// </summary>
public static class WeekMockData
{
    /// <summary>
    /// Represents a single schedule entry for the week view.
    /// </summary>
    public class ScheduleEntryDto
    {
        public int Id { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
        public string RoomNumber { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    /// <summary>
    /// Subject definition with color and icon for consistent styling.
    /// </summary>
    public class SubjectDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    /// <summary>
    /// Classroom definition.
    /// </summary>
    public class ClassroomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Teacher { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gets the list of 8 subjects with their styling.
    /// </summary>
    public static List<SubjectDefinition> GetSubjects() => new()
    {
        new() { Name = "Mathematics", Code = "MATH", Color = "#4CAF50", Icon = "Calculate" },
        new() { Name = "English", Code = "ENG", Color = "#2196F3", Icon = "MenuBook" },
        new() { Name = "Science", Code = "SCI", Color = "#9C27B0", Icon = "Science" },
        new() { Name = "History", Code = "HIST", Color = "#FF9800", Icon = "HistoryEdu" },
        new() { Name = "Geography", Code = "GEO", Color = "#00BCD4", Icon = "Public" },
        new() { Name = "Art", Code = "ART", Color = "#E91E63", Icon = "Palette" },
        new() { Name = "Music", Code = "MUS", Color = "#673AB7", Icon = "MusicNote" },
        new() { Name = "Physical Education", Code = "PE", Color = "#F44336", Icon = "FitnessCenter" }
    };

    /// <summary>
    /// Gets a mock classroom.
    /// </summary>
    public static ClassroomDto GetClassroom() => new()
    {
        Id = 1,
        Name = "Room 101",
        Grade = "Grade 5",
        Teacher = "Mrs. Johnson"
    };

    /// <summary>
    /// Gets mock schedule entries for a full week (Monday to Friday).
    /// Returns a realistic school timetable with 8 subjects spread across 5 days.
    /// </summary>
    public static async Task<List<ScheduleEntryDto>> GetWeekScheduleAsync(int classroomId = 1)
    {
        // Simulate API delay
        await Task.Delay(300);

        var subjects = GetSubjects();
        var entries = new List<ScheduleEntryDto>();
        var random = new Random(42); // Fixed seed for consistent mock data
        var teachers = new[] { "Mr. Smith", "Mrs. Williams", "Mr. Brown", "Ms. Davis", "Mr. Wilson", "Mrs. Taylor", "Mr. Anderson", "Ms. Martinez" };

        int id = 1;

        // Monday schedule
        entries.AddRange(CreateDaySchedule(id, DayOfWeek.Monday, subjects, teachers, random, ref id));
        
        // Tuesday schedule
        entries.AddRange(CreateDaySchedule(id, DayOfWeek.Tuesday, subjects, teachers, random, ref id));
        
        // Wednesday schedule
        entries.AddRange(CreateDaySchedule(id, DayOfWeek.Wednesday, subjects, teachers, random, ref id));
        
        // Thursday schedule
        entries.AddRange(CreateDaySchedule(id, DayOfWeek.Thursday, subjects, teachers, random, ref id));
        
        // Friday schedule
        entries.AddRange(CreateDaySchedule(id, DayOfWeek.Friday, subjects, teachers, random, ref id));

        return entries;
    }

    private static List<ScheduleEntryDto> CreateDaySchedule(
        int startId, 
        DayOfWeek day, 
        List<SubjectDefinition> subjects, 
        string[] teachers,
        Random random,
        ref int currentId)
    {
        var dayEntries = new List<ScheduleEntryDto>();
        
        // Define time slots based on day for variety
        var timeSlots = day switch
        {
            DayOfWeek.Monday => new[]
            {
                (new TimeSpan(8, 30, 0), 45, 0),  // Math
                (new TimeSpan(9, 20, 0), 45, 1),  // English
                (new TimeSpan(10, 20, 0), 45, 2), // Science (after break)
                (new TimeSpan(11, 10, 0), 45, 3), // History
                (new TimeSpan(13, 0, 0), 45, 4),  // Geography (after lunch)
                (new TimeSpan(13, 50, 0), 45, 6), // Music
            },
            DayOfWeek.Tuesday => new[]
            {
                (new TimeSpan(8, 30, 0), 45, 1),  // English
                (new TimeSpan(9, 20, 0), 45, 0),  // Math
                (new TimeSpan(10, 20, 0), 45, 5), // Art
                (new TimeSpan(11, 10, 0), 45, 2), // Science
                (new TimeSpan(13, 0, 0), 60, 7),  // PE (longer session)
                (new TimeSpan(14, 5, 0), 45, 3),  // History
            },
            DayOfWeek.Wednesday => new[]
            {
                (new TimeSpan(8, 30, 0), 45, 0),  // Math
                (new TimeSpan(9, 20, 0), 45, 2),  // Science
                (new TimeSpan(10, 20, 0), 45, 1), // English
                (new TimeSpan(11, 10, 0), 45, 4), // Geography
                (new TimeSpan(13, 0, 0), 45, 6),  // Music
                (new TimeSpan(13, 50, 0), 45, 5), // Art
            },
            DayOfWeek.Thursday => new[]
            {
                (new TimeSpan(8, 30, 0), 45, 2),  // Science
                (new TimeSpan(9, 20, 0), 45, 0),  // Math
                (new TimeSpan(10, 20, 0), 45, 3), // History
                (new TimeSpan(11, 10, 0), 45, 1), // English
                (new TimeSpan(13, 0, 0), 60, 7),  // PE
                (new TimeSpan(14, 5, 0), 45, 4),  // Geography
            },
            DayOfWeek.Friday => new[]
            {
                (new TimeSpan(8, 30, 0), 45, 1),  // English
                (new TimeSpan(9, 20, 0), 45, 0),  // Math
                (new TimeSpan(10, 20, 0), 45, 6), // Music
                (new TimeSpan(11, 10, 0), 45, 5), // Art
                (new TimeSpan(13, 0, 0), 45, 2),  // Science
                (new TimeSpan(13, 50, 0), 45, 4), // Geography
            },
            _ => Array.Empty<(TimeSpan, int, int)>()
        };

        foreach (var (startTime, duration, subjectIndex) in timeSlots)
        {
            var subject = subjects[subjectIndex];
            dayEntries.Add(new ScheduleEntryDto
            {
                Id = currentId++,
                SubjectName = subject.Name,
                SubjectCode = subject.Code,
                TeacherName = teachers[subjectIndex],
                Day = day,
                StartTime = startTime,
                EndTime = startTime.Add(TimeSpan.FromMinutes(duration)),
                RoomNumber = subjectIndex == 7 ? "Gym" : $"Room 10{subjectIndex + 1}",
                Color = subject.Color,
                Icon = subject.Icon
            });
        }

        return dayEntries;
    }

    /// <summary>
    /// Gets the time slots for the schedule grid.
    /// </summary>
    public static List<TimeSpan> GetTimeSlots()
    {
        var slots = new List<TimeSpan>();
        for (int hour = 8; hour <= 15; hour++)
        {
            slots.Add(new TimeSpan(hour, 0, 0));
            slots.Add(new TimeSpan(hour, 30, 0));
        }
        return slots;
    }
}
