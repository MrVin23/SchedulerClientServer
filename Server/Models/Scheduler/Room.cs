using Server.Models;

namespace Server.Models.Scheduler
{
    // School classes are identified by their room and year
    public class Room : ModelBase
    {
        public int Room { get; set; } // ie room 1
        public int Year { get; set; } // ie year 1
    }
}