using Server.Models;

namespace Server.Models.Scheduler
{
    public class Subject : ModelBase
    {
        public string SubjectName { get; set; } = string.Empty; // ie Maths, English, Science, etc.
        
        // Optional default duration for quick entry when creating schedule entries
        public int? DefaultDurationMinutes { get; set; } = 45;

        // Navigation property - all schedule entries for this subject
        public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
    }
}