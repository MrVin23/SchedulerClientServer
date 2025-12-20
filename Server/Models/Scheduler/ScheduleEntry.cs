using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Models.Scheduler
{
    /// <summary>
    /// Represents a single lesson occurrence in the weekly timetable.
    /// Each row is one period of a subject in a specific room on a specific day.
    /// This design allows variable period lengths and frequencies per subject.
    /// </summary>
    [Index(nameof(RoomId), nameof(DayOfWeek))]  // Fast lookup for room's daily schedule
    [Index(nameof(RoomId), nameof(DayOfWeek), nameof(StartTime), IsUnique = true)]  // Prevent double-booking
    public class ScheduleEntry : ModelBase
    {
        // Foreign key to Room (the classroom/year combination)
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        // Foreign key to Subject
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        // Day of the week (1 = Monday, 2 = Tuesday, ..., 5 = Friday)
        public int DayOfWeek { get; set; }

        // Start time of this period (e.g., 09:00, 10:30)
        public TimeSpan StartTime { get; set; }

        // Duration in minutes - can vary per entry
        public int DurationMinutes { get; set; }

        // Computed property for end time (not stored in DB)
        [NotMapped]
        public TimeSpan EndTime => StartTime.Add(TimeSpan.FromMinutes(DurationMinutes));
    }
}

