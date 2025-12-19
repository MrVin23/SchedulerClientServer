using Server.Models;

namespace Server.Models.Scheduler
{
    // This is a qualification that is required to assist a target student.
    // It should be called a Requirment for a target student, 
    // but we are using qualification to correlate with the qualification
    // needed by the the teacher aide.
    public class QualificationTargetStudent : ModelBase
    {
        public int QualificationId { get; set; }
        public Qualification Qualification { get; set; }
        public int TargetStudentsId { get; set; }
        public TargetStudents TargetStudents { get; set; }
    }
}