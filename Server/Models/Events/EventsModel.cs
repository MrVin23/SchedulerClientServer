using Server.Enums;
using Server.Models.Users;

namespace Server.Models.Events
{
    public class EventsModel : ModelBase
    {
        public int? EventTypeId { get; set; }
        public EventTypesModel? EventType { get; set; }
        public int? CreatedById { get; set; }
        public User? CreatedBy { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool CanBePostponed { get; set; } = true;
        public bool IsCompleted { get; set; } = false;
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }

    }
}