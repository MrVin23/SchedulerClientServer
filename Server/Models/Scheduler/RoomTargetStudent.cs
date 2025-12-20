using Server.Models;

namespace Server.Models.Scheduler
{
    public class RoomTargetStudent : ModelBase
    {
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        
        public int TargetStudentId { get; set; }
        public TargetStudent? TargetStudent { get; set; }
    }
}

