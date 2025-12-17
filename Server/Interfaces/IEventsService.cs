using Server.Dtos;
using Server.Models.Events;

namespace Server.Interfaces
{
    public interface IEventsService
    {
        // Event Types
        Task<IEnumerable<EventTypesModel>> GetAllEventTypesAsync();
        Task<EventTypesModel?> GetEventTypeByIdAsync(int id);
        Task<EventTypesModel?> GetEventTypeByNameAsync(string name);
        Task<EventTypesModel> CreateEventTypeAsync(CreateEventTypeRequest request);
        Task<EventTypesModel> UpdateEventTypeAsync(int id, UpdateEventTypeRequest request);
        Task<bool> DeleteEventTypeAsync(int id);
        Task<bool> EventTypeNameExistsAsync(string name);

        // Events
        Task<IEnumerable<EventsModel>> GetAllEventsAsync(int userId);
        Task<EventsModel?> GetEventByIdAsync(int id, int userId);
        Task<EventsModel> CreateEventAsync(CreateEventRequest request, int createdByUserId);
        Task<EventsModel> CreateEventWithUserAsync(CreateEventWithUserRequest request, int createdByUserId);
        Task<EventsModel> UpdateEventAsync(int id, UpdateEventRequest request);
        Task<BulkFollowUpEventsResponse> BulkFollowUpEventsAsync(BulkFollowUpEventsRequest request, int userId);
        Task<BulkPostponeEventsResponse> BulkPostponeEventsAsync(BulkPostponeEventsRequest request, int userId);
        Task<BulkRejectEventsResponse> BulkRejectEventsAsync(BulkRejectEventsRequest request, int userId);
        Task<BulkCompleteEventsResponse> BulkCompleteEventsAsync(BulkCompleteEventsRequest request, int userId);
        Task<EventsModel> ToggleEventCompletionAsync(int id, bool isCompleted);
        Task<bool> DeleteEventAsync(int id);
        Task<IEnumerable<EventsModel>> GetEventsByTypeAsync(int eventTypeId, int userId);
        Task<IEnumerable<EventsModel>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, int userId);
        Task<IEnumerable<EventsModel>> GetUpcomingEventsAsync(DateTime fromDate, int userId);
        Task<IEnumerable<EventsModel>> GetCompletedEventsAsync(int userId);
        Task<IEnumerable<EventsModel>> GetEventsByUserAsync(int userId);

        // User Events
        Task<IEnumerable<UserEventsModel>> GetUserEventsByUserIdAsync(int userId);
        Task<IEnumerable<UserEventsModel>> GetUserEventsByEventIdAsync(int eventId);
        Task<UserEventsModel?> GetUserEventAsync(int userId, int eventId);
        Task<UserEventsModel> CreateUserEventAsync(CreateUserEventRequest request);
        Task<bool> DeleteUserEventAsync(int userId, int eventId);
        Task<bool> UserHasEventAsync(int userId, int eventId);
    }
}

