using Server.Models;

namespace Server.Models.Scheduler
{
    public class TeacherAide : ModelBase
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Navigation property - qualifications this teacher aide has
        public ICollection<QualificationTeacherAide> QualificationTeacherAides { get; set; } = new List<QualificationTeacherAide>();
    }
}