using Server.Models;

namespace Server.Models.Scheduler
{
    public class TargetStudents : ModelBase
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsFunded { get; set; } = false;
        public bool IsDependent { get; set; } = false;
        public bool IsShareable { get; set; } = false;
        public int LevelOfDisruption { get; set; } = 0;
        public int LevelOfSupport { get; set; } = 0;
    }
}