using Server.Models;

namespace Server.Models.Scheduler
{
    /// <summary>
    /// This is a qualification that is required to assist a target student.
    /// It correlates with the qualification needed by the teacher aide.
    /// </summary>
    public class QualificationTargetStudent : ModelBase
    {
        public int QualificationId { get; set; }
        public Qualification? Qualification { get; set; }
        
        public int TargetStudentId { get; set; }
        public TargetStudent? TargetStudent { get; set; }
    }
}