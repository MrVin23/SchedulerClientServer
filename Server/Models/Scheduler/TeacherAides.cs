using namespace Server.Models;

namespace Server.Models.Scheduler
{
    public class TeacherAides : ModelBase
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}