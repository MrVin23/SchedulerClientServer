using Server.Models;

namespace Server.Models.Scheduler
{
    public class QualificationTeacherAide : ModelBase
    {
        public int QualificationId { get; set; }
        public Qualification? Qualification { get; set; }
        
        public int TeacherAideId { get; set; }
        public TeacherAide? TeacherAide { get; set; }
    }
}

