using System.Net.Http.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Client.Dtos;
using Client.Interfaces;

namespace Client.Services.HttpServices
{
    public class EventsService : IEventsService
    {
        private readonly HttpClient _httpClient;

        public EventsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Event Types

        /// <summary>
        /// Get all event types
        /// </summary>
        public async Task<ApiResponse<IEnumerable<EventTypeResponse>>?> GetAllEventTypesAsync()
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, "api/events/types");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<IEnumerable<EventTypeResponse>>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<IEnumerable<EventTypeResponse>>(ex.Message);
            }
        }

        /// <summary>
        /// Get event type by ID
        /// </summary>
        public async Task<ApiResponse<EventTypeResponse>?> GetEventTypeByIdAsync(int id)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, $"api/events/types/{id}");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<EventTypeResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventTypeResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Create a new event type
        /// </summary>
        public async Task<ApiResponse<EventTypeResponse>?> CreateEventTypeAsync(CreateEventTypeRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Post, "api/events/types", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<EventTypeResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventTypeResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing event type
        /// </summary>
        public async Task<ApiResponse<EventTypeResponse>?> UpdateEventTypeAsync(int id, UpdateEventTypeRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Put, $"api/events/types/{id}", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<EventTypeResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventTypeResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Delete an event type
        /// </summary>
        public async Task<bool> DeleteEventTypeAsync(int id)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Delete, $"api/events/types/{id}");
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if event type name exists
        /// </summary>
        public async Task<ApiResponse<bool>?> EventTypeNameExistsAsync(string name)
        {
            try
            {
                var encodedName = Uri.EscapeDataString(name);
                var request = CreateRequestWithCredentials(HttpMethod.Get, $"api/events/types/exists/{encodedName}");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<bool>(ex.Message);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Get all events
        /// </summary>
        public async Task<ApiResponse<IEnumerable<EventResponse>>?> GetAllEventsAsync()
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, "api/events");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<IEnumerable<EventResponse>>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<IEnumerable<EventResponse>>(ex.Message);
            }
        }

        /// <summary>
        /// Get event by ID
        /// </summary>
        public async Task<ApiResponse<EventResponse>?> GetEventByIdAsync(int id)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, $"api/events/{id}");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<EventResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Create a new event
        /// </summary>
        public async Task<ApiResponse<EventResponse>?> CreateEventAsync(CreateEventRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Post, "api/events", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<EventResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing event
        /// </summary>
        public async Task<ApiResponse<EventResponse>?> UpdateEventAsync(int id, UpdateEventRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Put, $"api/events/{id}", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<EventResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Delete an event
        /// </summary>
        public async Task<bool> DeleteEventAsync(int id)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Delete, $"api/events/{id}");
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get events by type
        /// </summary>
        public async Task<ApiResponse<IEnumerable<EventResponse>>?> GetEventsByTypeAsync(int eventTypeId)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, $"api/events/type/{eventTypeId}");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<IEnumerable<EventResponse>>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<IEnumerable<EventResponse>>(ex.Message);
            }
        }

        /// <summary>
        /// Get events by date range
        /// </summary>
        public async Task<ApiResponse<IEnumerable<EventResponse>>?> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var queryParams = $"?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}";
                var request = CreateRequestWithCredentials(HttpMethod.Get, $"api/events/date-range{queryParams}");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<IEnumerable<EventResponse>>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<IEnumerable<EventResponse>>(ex.Message);
            }
        }

        /// <summary>
        /// Get upcoming events
        /// </summary>
        public async Task<ApiResponse<IEnumerable<EventResponse>>?> GetUpcomingEventsAsync(DateTime? fromDate = null)
        {
            try
            {
                var url = "api/events/upcoming";
                if (fromDate.HasValue)
                {
                    url += $"?fromDate={fromDate.Value:yyyy-MM-ddTHH:mm:ss}";
                }
                var request = CreateRequestWithCredentials(HttpMethod.Get, url);
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<IEnumerable<EventResponse>>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<IEnumerable<EventResponse>>(ex.Message);
            }
        }

        /// <summary>
        /// Get completed events
        /// </summary>
        public async Task<ApiResponse<IEnumerable<EventResponse>>?> GetCompletedEventsAsync()
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, "api/events/completed");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<IEnumerable<EventResponse>>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<IEnumerable<EventResponse>>(ex.Message);
            }
        }

        /// <summary>
        /// Get events by user
        /// </summary>
        public async Task<ApiResponse<IEnumerable<EventResponse>>?> GetEventsByUserAsync(int userId)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, $"api/events/user/{userId}");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<IEnumerable<EventResponse>>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<IEnumerable<EventResponse>>(ex.Message);
            }
        }

        /// <summary>
        /// Create a new event and link it to a user
        /// </summary>
        public async Task<ApiResponse<EventResponse>?> CreateEventWithUserAsync(CreateEventWithUserRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Post, "api/events/with-user", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<EventResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Follow up multiple events by adding follow-up period days to their start and end dates
        /// </summary>
        /// <param name="request">Request containing list of event IDs to follow up</param>
        public async Task<ApiResponse<BulkFollowUpEventsResponse>?> BulkFollowUpEventsAsync(BulkFollowUpEventsRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Put, "api/events/follow-up", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<BulkFollowUpEventsResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<BulkFollowUpEventsResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Postpone multiple events by adding exactly one day to their start and end dates
        /// </summary>
        /// <param name="request">Request containing list of event IDs to postpone</param>
        public async Task<ApiResponse<BulkPostponeEventsResponse>?> BulkPostponeEventsAsync(BulkPostponeEventsRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Put, "api/events/postpone", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<BulkPostponeEventsResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<BulkPostponeEventsResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Reject multiple events by removing the user-event links (unlinking the user from events)
        /// </summary>
        /// <param name="request">Request containing list of event IDs to reject</param>
        public async Task<ApiResponse<BulkRejectEventsResponse>?> BulkRejectEventsAsync(BulkRejectEventsRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Put, "api/events/reject", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<BulkRejectEventsResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<BulkRejectEventsResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Mark multiple events as completed by setting the IsCompleted flag to true
        /// </summary>
        /// <param name="request">Request containing list of event IDs to mark as completed</param>
        public async Task<ApiResponse<BulkCompleteEventsResponse>?> BulkCompleteEventsAsync(BulkCompleteEventsRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Put, "api/events/complete", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<BulkCompleteEventsResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<BulkCompleteEventsResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Toggle completion status of a single event (set to complete or incomplete)
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="request">Request containing the desired completion status</param>
        public async Task<ApiResponse<EventResponse>?> ToggleEventCompletionAsync(int id, ToggleEventCompletionRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Put, $"api/events/{id}/toggle-completion", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<EventResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventResponse>(ex.Message);
            }
        }

        #endregion

        #region User Events

        /// <summary>
        /// Get all user-event associations for a user
        /// </summary>
        public async Task<ApiResponse<IEnumerable<UserEventResponse>>?> GetUserEventsByUserIdAsync(int userId)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, $"api/events/users/{userId}");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<IEnumerable<UserEventResponse>>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<IEnumerable<UserEventResponse>>(ex.Message);
            }
        }

        /// <summary>
        /// Get all user-event associations for an event
        /// </summary>
        public async Task<ApiResponse<IEnumerable<UserEventResponse>>?> GetUserEventsByEventIdAsync(int eventId)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, $"api/events/{eventId}/users");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<IEnumerable<UserEventResponse>>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<IEnumerable<UserEventResponse>>(ex.Message);
            }
        }

        /// <summary>
        /// Link a user to an event
        /// </summary>
        public async Task<ApiResponse<UserEventResponse>?> CreateUserEventAsync(CreateUserEventRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Post, "api/events/users", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<UserEventResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<UserEventResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Unlink a user from an event
        /// </summary>
        public async Task<bool> DeleteUserEventAsync(int userId, int eventId)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Delete, $"api/events/users/{userId}/events/{eventId}");
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if user is linked to an event
        /// </summary>
        public async Task<ApiResponse<bool>?> UserHasEventAsync(int userId, int eventId)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, $"api/events/users/{userId}/events/{eventId}/exists");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<bool>(ex.Message);
            }
        }

        #endregion

        #region Helper Methods

        private HttpRequestMessage CreateRequestWithCredentials(HttpMethod method, string uri, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, uri);
            if (content != null)
            {
                request.Content = content;
            }
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            return request;
        }

        private async Task<ApiResponse<T>?> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ApiError>();
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = error?.Message ?? $"Request failed with status: {response.StatusCode}",
                    Data = default
                };
            }
        }

        private ApiResponse<T> CreateErrorResponse<T>(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"An error occurred: {message}",
                Data = default
            };
        }

        #endregion
    }
}

