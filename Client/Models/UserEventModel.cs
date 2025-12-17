using Client.Enums;

namespace Client.Models;

public class UserEventModel
{
    public int Id { get; set; }
    public EventTypeEnums EventType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool CanBePostponed { get; set; } = true;
    public bool IsSelected { get; set; } = false;
    public bool IsCompleted { get; set; } = false;
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
}

