using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Models.Scheduler
{
    // School classes are identified by their room and year
    [Index(nameof(RoomNumber), nameof(Year), IsUnique = true)]
    public class Room : ModelBase
    {
        public int RoomNumber { get; set; } // ie room 1
        public int Year { get; set; } // ie year 1

        // Navigation properties
        public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
        public ICollection<RoomTargetStudent> RoomTargetStudents { get; set; } = new List<RoomTargetStudent>();
    }
}