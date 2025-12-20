using Server.Models;

namespace Server.Models.Scheduler
{
    public class Qualification : ModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<QualificationTeacherAide> QualificationTeacherAides { get; set; } = new List<QualificationTeacherAide>();
        public ICollection<QualificationTargetStudent> QualificationTargetStudents { get; set; } = new List<QualificationTargetStudent>();
    }
}