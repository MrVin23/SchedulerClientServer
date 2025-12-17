using Client.Dtos;

namespace Client.Interfaces
{
    public interface IEventsService
    {
        #region Event Types

        /// <summary>
        /// Get all event types
        /// </summary>
        Task<ApiResponse<IEnumerable<EventTypeResponse>>?> GetAllEventTypesAsync();

        /// <summary>
        /// Get event type by ID
        /// </summary>
        Task<ApiResponse<EventTypeResponse>?> GetEventTypeByIdAsync(int id);

        /// <summary>
        /// Create a new event type
        /// </summary>
        Task<ApiResponse<EventTypeResponse>?> CreateEventTypeAsync(CreateEventTypeRequest request);

        /// <summary>
        /// Update an existing event type
        /// </summary>
        Task<ApiResponse<EventTypeResponse>?> UpdateEventTypeAsync(int id, UpdateEventTypeRequest request);

        /// <summary>
        /// Delete an event type
        /// </summary>
        Task<bool> DeleteEventTypeAsync(int id);

        /// <summary>
        /// Check if event type name exists
        /// </summary>
        Task<ApiResponse<bool>?> EventTypeNameExistsAsync(string name);

        #endregion

        #region Events

        /// <summary>
        /// Get all events
        /// </summary>
        Task<ApiResponse<IEnumerable<EventResponse>>?> GetAllEventsAsync();

        /// <summary>
        /// Get event by ID
        /// </summary>
        Task<ApiResponse<EventResponse>?> GetEventByIdAsync(int id);

        /// <summary>
        /// Create a new event
        /// </summary>
        Task<ApiResponse<EventResponse>?> CreateEventAsync(CreateEventRequest request);

        /// <summary>
        /// Update an existing event
        /// </summary>
        Task<ApiResponse<EventResponse>?> UpdateEventAsync(int id, UpdateEventRequest request);

        /// <summary>
        /// Delete an event
        /// </summary>
        Task<bool> DeleteEventAsync(int id);

        /// <summary>
        /// Get events by type
        /// </summary>
        Task<ApiResponse<IEnumerable<EventResponse>>?> GetEventsByTypeAsync(int eventTypeId);

        /// <summary>
        /// Get events by date range
        /// </summary>
        Task<ApiResponse<IEnumerable<EventResponse>>?> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get upcoming events
        /// </summary>
        Task<ApiResponse<IEnumerable<EventResponse>>?> GetUpcomingEventsAsync(DateTime? fromDate = null);

        /// <summary>
        /// Get completed events
        /// </summary>
        Task<ApiResponse<IEnumerable<EventResponse>>?> GetCompletedEventsAsync();

        /// <summary>
        /// Get events by user
        /// </summary>
        Task<ApiResponse<IEnumerable<EventResponse>>?> GetEventsByUserAsync(int userId);

        /// <summary>
        /// Create a new event and link it to a user
        /// </summary>
        Task<ApiResponse<EventResponse>?> CreateEventWithUserAsync(CreateEventWithUserRequest request);

        /// <summary>
        /// Follow up multiple events by adding follow-up period days to their start and end dates
        /// </summary>
        /// <param name="request">Request containing list of event IDs to follow up</param>
        Task<ApiResponse<BulkFollowUpEventsResponse>?> BulkFollowUpEventsAsync(BulkFollowUpEventsRequest request);

        /// <summary>
        /// Postpone multiple events by adding exactly one day to their start and end dates
        /// </summary>
        /// <param name="request">Request containing list of event IDs to postpone</param>
        Task<ApiResponse<BulkPostponeEventsResponse>?> BulkPostponeEventsAsync(BulkPostponeEventsRequest request);

        /// <summary>
        /// Reject multiple events by removing the user-event links (unlinking the user from events)
        /// </summary>
        /// <param name="request">Request containing list of event IDs to reject</param>
        Task<ApiResponse<BulkRejectEventsResponse>?> BulkRejectEventsAsync(BulkRejectEventsRequest request);

        /// <summary>
        /// Mark multiple events as completed by setting the IsCompleted flag to true
        /// </summary>
        /// <param name="request">Request containing list of event IDs to mark as completed</param>
        Task<ApiResponse<BulkCompleteEventsResponse>?> BulkCompleteEventsAsync(BulkCompleteEventsRequest request);

        /// <summary>
        /// Toggle completion status of a single event (set to complete or incomplete)
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="request">Request containing the desired completion status</param>
        Task<ApiResponse<EventResponse>?> ToggleEventCompletionAsync(int id, ToggleEventCompletionRequest request);

        #endregion

        #region User Events

        /// <summary>
        /// Get all user-event associations for a user
        /// </summary>
        Task<ApiResponse<IEnumerable<UserEventResponse>>?> GetUserEventsByUserIdAsync(int userId);

        /// <summary>
        /// Get all user-event associations for an event
        /// </summary>
        Task<ApiResponse<IEnumerable<UserEventResponse>>?> GetUserEventsByEventIdAsync(int eventId);

        /// <summary>
        /// Link a user to an event
        /// </summary>
        Task<ApiResponse<UserEventResponse>?> CreateUserEventAsync(CreateUserEventRequest request);

        /// <summary>
        /// Unlink a user from an event
        /// </summary>
        Task<bool> DeleteUserEventAsync(int userId, int eventId);

        /// <summary>
        /// Check if user is linked to an event
        /// </summary>
        Task<ApiResponse<bool>?> UserHasEventAsync(int userId, int eventId);

        #endregion
    }
}

