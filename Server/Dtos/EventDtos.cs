namespace Server.Dtos
{
    public class CreateEventRequest
    {
        public int? EventTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool CanBePostponed { get; set; } = true;
        public bool IsCompleted { get; set; } = false;
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }

    public class UpdateEventRequest
    {
        public int? EventTypeId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? CanBePostponed { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }

    public class EventResponse
    {
        public int Id { get; set; }
        public int? EventTypeId { get; set; }
        public string? EventTypeName { get; set; }
        public int? CreatedById { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool CanBePostponed { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateEventTypeRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateEventTypeRequest
    {
        public string? Name { get; set; }
    }

    public class EventTypeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateUserEventRequest
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
    }

    public class CreateEventWithUserRequest
    {
        public int? EventTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool CanBePostponed { get; set; } = true;
        public bool IsCompleted { get; set; } = false;
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int UserId { get; set; }
    }

    public class UserEventResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int EventId { get; set; }
        public string? EventTitle { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateEventSettingsRequest
    {
        public int FollowUpPeriodDays { get; set; }
    }

    public class UpdateEventSettingsRequest
    {
        public int? FollowUpPeriodDays { get; set; }
    }

    public class EventSettingsResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int FollowUpPeriodDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BulkFollowUpEventsRequest
    {
        public List<int> EventIds { get; set; } = new List<int>();
    }

    public class BulkFollowUpEventsResponse
    {
        public List<EventResponse> SuccessfulEvents { get; set; } = new List<EventResponse>();
        public List<BulkFollowUpError> FailedEvents { get; set; } = new List<BulkFollowUpError>();
    }

    public class BulkFollowUpError
    {
        public int EventId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class BulkPostponeEventsRequest
    {
        public List<int> EventIds { get; set; } = new List<int>();
    }

    public class BulkPostponeEventsResponse
    {
        public List<EventResponse> SuccessfulEvents { get; set; } = new List<EventResponse>();
        public List<BulkPostponeError> FailedEvents { get; set; } = new List<BulkPostponeError>();
    }

    public class BulkPostponeError
    {
        public int EventId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class BulkRejectEventsRequest
    {
        public List<int> EventIds { get; set; } = new List<int>();
    }

    public class BulkRejectEventsResponse
    {
        public List<BulkRejectSuccess> SuccessfulRejections { get; set; } = new List<BulkRejectSuccess>();
        public List<BulkRejectError> FailedRejections { get; set; } = new List<BulkRejectError>();
    }

    public class BulkRejectSuccess
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
    }

    public class BulkRejectError
    {
        public int EventId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class BulkCompleteEventsRequest
    {
        public List<int> EventIds { get; set; } = new List<int>();
    }

    public class BulkCompleteEventsResponse
    {
        public List<EventResponse> SuccessfulCompletions { get; set; } = new List<EventResponse>();
        public List<BulkCompleteError> FailedCompletions { get; set; } = new List<BulkCompleteError>();
    }

    public class BulkCompleteError
    {
        public int EventId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ToggleEventCompletionRequest
    {
        public bool IsCompleted { get; set; }
    }
}

