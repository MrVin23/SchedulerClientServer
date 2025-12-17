using Server.Models.Users;

namespace Server.Models.Events
{
    public class EventSettingsModel : ModelBase
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int FollowUpPeriodDays { get; set; }
    }
}