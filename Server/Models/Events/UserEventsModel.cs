using Server.Models.Users;

namespace Server.Models.Events
{
    public class UserEventsModel : ModelBase
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int EventId { get; set; }
        public EventsModel Event { get; set; } = null!;
    }
}