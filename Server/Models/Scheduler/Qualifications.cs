using Server.Models;

namespace Server.Models.Scheduler
{
    public class Qualifications : ModelBase
    {
        public string Qualification { get; set; } = 0;
        public string Description { get; set; } = 0;
    }
}