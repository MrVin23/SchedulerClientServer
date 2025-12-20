using Server.Models;

namespace Server.Models.Scheduler
{
    // School classes are identified by their room and year
    public class Room : ModelBase
    {
        public int RoomNumber { get; set; } // ie room 1
        public int Year { get; set; } // ie year 1

        // Navigation property - all schedule entries for this room
        public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
    }
}