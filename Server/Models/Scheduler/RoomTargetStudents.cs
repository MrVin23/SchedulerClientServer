using Server.Models;

namespace Server.Models.Scheduler
{
    public class RoomTargetStudents : ModelBase
    {
        public int RoomId { get; set; }
        public Room Room { get; set; }
        public int TargetStudentsId { get; set; }
        public TargetStudents TargetStudents { get; set; }
    }
}