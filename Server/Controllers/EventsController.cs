using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Controllers;
using Server.Dtos;
using Server.Interfaces;
using Server.Services;
using Server.Utils;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ActiveUser")]
    public class EventsController : BaseController
    {
        private readonly IEventsService _eventsService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventsService eventsService, ILogger<EventsController> logger)
        {
            _eventsService = eventsService;
            _logger = logger;
        }

        #region Event Types

        /// <summary>
        /// Get all event types
        /// </summary>
        /// <returns>List of all event types</returns>
        [HttpGet("types")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventTypeResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAllEventTypes()
        {
            var eventTypes = await _eventsService.GetAllEventTypesAsync();
            var responses = eventTypes.Select(et => ((EventsService)_eventsService).MapToEventTypeResponse(et));
            return SuccessResponse(responses, "Event types retrieved successfully");
        }

        /// <summary>
        /// Get event type by ID
        /// </summary>
        /// <param name="id">Event type ID</param>
        /// <returns>Event type details</returns>
        [HttpGet("types/{id}")]
        [ProducesResponseType(typeof(ApiResponse<EventTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetEventTypeById(int id)
        {
            var eventType = await _eventsService.GetEventTypeByIdAsync(id);
            if (eventType == null)
            {
                return NotFoundResponse($"Event type with ID {id} not found");
            }

            var response = ((EventsService)_eventsService).MapToEventTypeResponse(eventType);
            return SuccessResponse(response, "Event type retrieved successfully");
        }

        /// <summary>
        /// Create a new event type
        /// </summary>
        /// <param name="request">Event type creation request</param>
        /// <returns>Created event type</returns>
        [HttpPost("types")]
        [ProducesResponseType(typeof(ApiResponse<EventTypeResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateEventType([FromBody] CreateEventTypeRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var eventType = await _eventsService.CreateEventTypeAsync(request);
                var response = ((EventsService)_eventsService).MapToEventTypeResponse(eventType);
                var location = Url.Action(nameof(GetEventTypeById), new { id = eventType.Id }) ?? string.Empty;
                return CreatedResponse(response, location, "Event type created successfully");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing event type
        /// </summary>
        /// <param name="id">Event type ID</param>
        /// <param name="request">Event type update request</param>
        /// <returns>Updated event type</returns>
        [HttpPut("types/{id}")]
        [ProducesResponseType(typeof(ApiResponse<EventTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateEventType(int id, [FromBody] UpdateEventTypeRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var eventType = await _eventsService.UpdateEventTypeAsync(id, request);
                var response = ((EventsService)_eventsService).MapToEventTypeResponse(eventType);
                return SuccessResponse(response, "Event type updated successfully");
            }
            catch (ArgumentException ex)
            {
                return NotFoundResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        /// <summary>
        /// Delete an event type
        /// </summary>
        /// <param name="id">Event type ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("types/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteEventType(int id)
        {
            var deleted = await _eventsService.DeleteEventTypeAsync(id);
            if (!deleted)
            {
                return NotFoundResponse($"Event type with ID {id} not found");
            }

            return NoContent();
        }

        /// <summary>
        /// Check if event type name exists
        /// </summary>
        /// <param name="name">Event type name to check</param>
        /// <returns>Existence result</returns>
        [HttpGet("types/exists/{name}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> EventTypeNameExists(string name)
        {
            var exists = await _eventsService.EventTypeNameExistsAsync(name);
            return SuccessResponse(exists, $"Event type name '{name}' {(exists ? "exists" : "does not exist")}");
        }

        #endregion

        #region Events

        /// <summary>
        /// Get all events
        /// </summary>
        /// <returns>List of all events</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAllEvents()
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == 0)
            {
                return BadRequestResponse("User authentication required to view events");
            }

            var events = await _eventsService.GetAllEventsAsync(currentUserId);
            var responses = events.Select(e => ((EventsService)_eventsService).MapToEventResponse(e));
            return SuccessResponse(responses, "Events retrieved successfully");
        }

        /// <summary>
        /// Get event by ID
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>Event details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<EventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetEventById(int id)
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == 0)
            {
                return BadRequestResponse("User authentication required to view events");
            }

            var eventModel = await _eventsService.GetEventByIdAsync(id, currentUserId);
            if (eventModel == null)
            {
                return NotFoundResponse($"Event with ID {id} not found or you don't have permission to view it");
            }

            var response = ((EventsService)_eventsService).MapToEventResponse(eventModel);
            return SuccessResponse(response, "Event retrieved successfully");
        }

        /// <summary>
        /// Create a new event
        /// </summary>
        /// <param name="request">Event creation request</param>
        /// <returns>Created event</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EventResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == 0)
                {
                    return BadRequestResponse("User authentication required to create events");
                }

                var eventModel = await _eventsService.CreateEventAsync(request, currentUserId);
                var response = ((EventsService)_eventsService).MapToEventResponse(eventModel);
                var location = Url.Action(nameof(GetEventById), new { id = eventModel.Id }) ?? string.Empty;
                return CreatedResponse(response, location, "Event created successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        /// <summary>
        /// Create a new event and link a user to it
        /// </summary>
        /// <param name="request">Event creation with user linking request. Note: All DateTime values must be in UTC format.</param>
        /// <returns>Created event</returns>
        [HttpPost("with-user")]
        [ProducesResponseType(typeof(ApiResponse<EventResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateEventWithUser([FromBody] CreateEventWithUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == 0)
                {
                    return BadRequestResponse("User authentication required to create events");
                }

                var eventModel = await _eventsService.CreateEventWithUserAsync(request, currentUserId);
                var response = ((EventsService)_eventsService).MapToEventResponse(eventModel);
                var location = Url.Action(nameof(GetEventById), new { id = eventModel.Id }) ?? string.Empty;
                return CreatedResponse(response, location, "Event created and user linked successfully");
            }
            catch (ArgumentException ex)
            {
                // Handle validation errors (user not found, event type not found, date validation, etc.)
                var message = ex.Message;

                // Provide more specific messages for common validation errors
                if (message.Contains("does not exist"))
                {
                    return BadRequestResponse($"Validation error: {message}");
                }
                else if (message.Contains("start date must be before"))
                {
                    return BadRequestResponse($"Validation error: {message}");
                }
                else if (message.Contains("required and cannot be empty"))
                {
                    return BadRequestResponse($"Validation error: {message}");
                }

                return BadRequestResponse($"Validation error: {message}");
            }
            catch (DbUpdateException ex)
            {
                // Handle database constraint violations (foreign key constraints, unique constraints, etc.)
                var innerMessage = ex.InnerException?.Message ?? ex.Message;

                // Check for DateTime UTC conversion errors first
                if (innerMessage.Contains("Cannot write DateTime with Kind=Local to PostgreSQL") ||
                    innerMessage.Contains("only UTC is supported"))
                {
                    return BadRequestResponse("Invalid date/time format. All dates must be in UTC format.");
                }

                if (innerMessage.Contains("FOREIGN KEY") || innerMessage.Contains("constraint"))
                {
                    if (innerMessage.Contains("FK_UserEvents_User") || innerMessage.Contains("UserId"))
                    {
                        return BadRequestResponse("Invalid user ID. The specified user does not exist.");
                    }
                    if (innerMessage.Contains("FK_UserEvents_Event") || innerMessage.Contains("EventId"))
                    {
                        return BadRequestResponse("Invalid event ID. The event could not be created.");
                    }
                    if (innerMessage.Contains("FK_Events_EventType") || innerMessage.Contains("EventTypeId"))
                    {
                        return BadRequestResponse("Invalid event type ID. The specified event type does not exist.");
                    }

                    return BadRequestResponse("Database constraint violation. Please check that all referenced entities exist.");
                }

                if (innerMessage.Contains("UNIQUE") || innerMessage.Contains("duplicate") ||
                    innerMessage.Contains("IX_UserEvents_UserId_EventId"))
                {
                    return BadRequestResponse("This user is already linked to an event. Each user can only be linked to each event once.");
                }

                _logger.LogError(ex, "Database error occurred while creating event with user. Inner exception: {InnerException}", ex.InnerException?.Message);
                return InternalServerErrorResponse("A database error occurred while processing your request. Please try again.");
            }
            catch (InvalidOperationException ex)
            {
                // Handle business logic violations
                return BadRequestResponse($"Operation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                _logger.LogError(ex, "Unexpected error occurred while creating event with user");
                return InternalServerErrorResponse("An unexpected error occurred while processing your request. Please try again later.");
            }
        }

        /// <summary>
        /// Update an existing event
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="request">Event update request</param>
        /// <returns>Updated event</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<EventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == 0)
                {
                    return BadRequestResponse("User authentication required to update events");
                }

                // Check if the current user is the owner of the event
                var existingEvent = await _eventsService.GetEventByIdAsync(id, currentUserId);
                if (existingEvent == null)
                {
                    return NotFoundResponse($"Event with ID {id} not found or you don't have permission to update it");
                }

                var eventModel = await _eventsService.UpdateEventAsync(id, request);
                var response = ((EventsService)_eventsService).MapToEventResponse(eventModel);
                return SuccessResponse(response, "Event updated successfully");
            }
            catch (ArgumentException ex)
            {
                return NotFoundResponse(ex.Message);
            }
        }

        /// <summary>
        /// Follow up multiple events by adding follow-up period days to their start and end dates
        /// </summary>
        /// <param name="request">Request containing list of event IDs to follow up</param>
        /// <returns>Result showing successful and failed follow-ups</returns>
        [HttpPut("follow-up")]
        [ProducesResponseType(typeof(ApiResponse<BulkFollowUpEventsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> BulkFollowUpEvents([FromBody] BulkFollowUpEventsRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == 0)
                {
                    return BadRequestResponse("User authentication required to follow up events");
                }

                if (request.EventIds == null || !request.EventIds.Any())
                {
                    return BadRequestResponse("At least one event ID must be provided");
                }

                var result = await _eventsService.BulkFollowUpEventsAsync(request, currentUserId);
                var message = $"Follow-up completed. {result.SuccessfulEvents.Count} events updated successfully, {result.FailedEvents.Count} failed.";

                return SuccessResponse(result, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during bulk follow-up");
                return InternalServerErrorResponse("An unexpected error occurred while processing your request. Please try again later.");
            }
        }

        /// <summary>
        /// Postpone multiple events by adding exactly one day to their start and end dates
        /// </summary>
        /// <param name="request">Request containing list of event IDs to postpone</param>
        /// <returns>Result showing successful and failed postponements</returns>
        [HttpPut("postpone")]
        [ProducesResponseType(typeof(ApiResponse<BulkPostponeEventsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> BulkPostponeEvents([FromBody] BulkPostponeEventsRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == 0)
                {
                    return BadRequestResponse("User authentication required to postpone events");
                }

                if (request.EventIds == null || !request.EventIds.Any())
                {
                    return BadRequestResponse("At least one event ID must be provided");
                }

                var result = await _eventsService.BulkPostponeEventsAsync(request, currentUserId);
                var message = $"Postponement completed. {result.SuccessfulEvents.Count} events updated successfully, {result.FailedEvents.Count} failed.";

                return SuccessResponse(result, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during bulk postponement");
                return InternalServerErrorResponse("An unexpected error occurred while processing your request. Please try again later.");
            }
        }

        /// <summary>
        /// Reject multiple events by removing the user-event links (unlinking the user from events)
        /// </summary>
        /// <param name="request">Request containing list of event IDs to reject</param>
        /// <returns>Result showing successful and failed rejections</returns>
        [HttpPut("reject")]
        [ProducesResponseType(typeof(ApiResponse<BulkRejectEventsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> BulkRejectEvents([FromBody] BulkRejectEventsRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == 0)
                {
                    return BadRequestResponse("User authentication required to reject events");
                }

                if (request.EventIds == null || !request.EventIds.Any())
                {
                    return BadRequestResponse("At least one event ID must be provided");
                }

                var result = await _eventsService.BulkRejectEventsAsync(request, currentUserId);
                var message = $"Rejection completed. {result.SuccessfulRejections.Count} events rejected successfully, {result.FailedRejections.Count} failed.";

                return SuccessResponse(result, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during bulk rejection");
                return InternalServerErrorResponse("An unexpected error occurred while processing your request. Please try again later.");
            }
        }

        /// <summary>
        /// Mark multiple events as completed by setting the IsCompleted flag to true
        /// </summary>
        /// <param name="request">Request containing list of event IDs to mark as completed</param>
        /// <returns>Result showing successful completions and failed operations</returns>
        [HttpPut("complete")]
        [ProducesResponseType(typeof(ApiResponse<BulkCompleteEventsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> BulkCompleteEvents([FromBody] BulkCompleteEventsRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == 0)
                {
                    return BadRequestResponse("User authentication required to complete events");
                }

                if (request.EventIds == null || !request.EventIds.Any())
                {
                    return BadRequestResponse("At least one event ID must be provided");
                }

                var result = await _eventsService.BulkCompleteEventsAsync(request, currentUserId);
                var message = $"Completion completed. {result.SuccessfulCompletions.Count} events marked as completed successfully, {result.FailedCompletions.Count} failed.";

                return SuccessResponse(result, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during bulk completion");
                return InternalServerErrorResponse("An unexpected error occurred while processing your request. Please try again later.");
            }
        }

        /// <summary>
        /// Toggle completion status of a single event (set to complete or incomplete)
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="request">Request containing the desired completion status</param>
        /// <returns>Updated event details</returns>
        [HttpPut("{id}/toggle-completion")]
        [ProducesResponseType(typeof(ApiResponse<EventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ToggleEventCompletion(int id, [FromBody] ToggleEventCompletionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var currentUserId = User.GetUserId();
                if (currentUserId == 0)
                {
                    return BadRequestResponse("User authentication required to toggle event completion");
                }

                var eventModel = await _eventsService.ToggleEventCompletionAsync(id, request.IsCompleted);
                var response = ((EventsService)_eventsService).MapToEventResponse(eventModel);
                var statusText = request.IsCompleted ? "completed" : "incomplete";
                return SuccessResponse(response, $"Event marked as {statusText} successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequestResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while toggling event completion");
                return InternalServerErrorResponse("An unexpected error occurred while processing your request. Please try again later.");
            }
        }

        /// <summary>
        /// Delete an event
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == 0)
            {
                return BadRequestResponse("User authentication required to delete events");
            }

            // Check if the current user is the owner of the event
            var existingEvent = await _eventsService.GetEventByIdAsync(id, currentUserId);
            if (existingEvent == null)
            {
                return NotFoundResponse($"Event with ID {id} not found or you don't have permission to delete it");
            }

            var deleted = await _eventsService.DeleteEventAsync(id);
            if (!deleted)
            {
                return NotFoundResponse($"Event with ID {id} not found");
            }

            return NoContent();
        }

        /// <summary>
        /// Get events by type
        /// </summary>
        /// <param name="eventTypeId">Event type ID</param>
        /// <returns>List of events with the specified type</returns>
        [HttpGet("type/{eventTypeId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetEventsByType(int eventTypeId)
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == 0)
            {
                return BadRequestResponse("User authentication required to view events");
            }

            var events = await _eventsService.GetEventsByTypeAsync(eventTypeId, currentUserId);
            var responses = events.Select(e => ((EventsService)_eventsService).MapToEventResponse(e));
            return SuccessResponse(responses, $"Events with type ID {eventTypeId} retrieved successfully");
        }

        /// <summary>
        /// Get events by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of events within the date range</returns>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetEventsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == 0)
            {
                return BadRequestResponse("User authentication required to view events");
            }

            if (startDate > endDate)
            {
                return BadRequestResponse("Start date must be before or equal to end date");
            }

            var events = await _eventsService.GetEventsByDateRangeAsync(startDate, endDate, currentUserId);
            var responses = events.Select(e => ((EventsService)_eventsService).MapToEventResponse(e));
            return SuccessResponse(responses, "Events retrieved successfully");
        }

        /// <summary>
        /// Get upcoming events
        /// </summary>
        /// <param name="fromDate">Start date for upcoming events (defaults to now)</param>
        /// <returns>List of upcoming events</returns>
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetUpcomingEvents([FromQuery] DateTime? fromDate = null)
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == 0)
            {
                return BadRequestResponse("User authentication required to view events");
            }

            var date = fromDate ?? DateTime.UtcNow;
            var events = await _eventsService.GetUpcomingEventsAsync(date, currentUserId);
            var responses = events.Select(e => ((EventsService)_eventsService).MapToEventResponse(e));
            return SuccessResponse(responses, "Upcoming events retrieved successfully");
        }

        /// <summary>
        /// Get completed events
        /// </summary>
        /// <returns>List of completed events</returns>
        [HttpGet("completed")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetCompletedEvents()
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == 0)
            {
                return BadRequestResponse("User authentication required to view events");
            }

            var events = await _eventsService.GetCompletedEventsAsync(currentUserId);
            var responses = events.Select(e => ((EventsService)_eventsService).MapToEventResponse(e));
            return SuccessResponse(responses, "Completed events retrieved successfully");
        }

        /// <summary>
        /// Get events by user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of events for the specified user</returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetEventsByUser(int userId)
        {
            var events = await _eventsService.GetEventsByUserAsync(userId);
            var responses = events.Select(e => ((EventsService)_eventsService).MapToEventResponse(e));
            return SuccessResponse(responses, $"Events for user ID {userId} retrieved successfully");
        }

        #endregion

        #region User Events

        /// <summary>
        /// Get all user-event associations for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user-event associations</returns>
        [HttpGet("users/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserEventResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetUserEventsByUserId(int userId)
        {
            var userEvents = await _eventsService.GetUserEventsByUserIdAsync(userId);
            var responses = userEvents.Select(ue => ((EventsService)_eventsService).MapToUserEventResponse(ue));
            return SuccessResponse(responses, $"User events for user ID {userId} retrieved successfully");
        }

        /// <summary>
        /// Get all user-event associations for an event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>List of user-event associations</returns>
        [HttpGet("{eventId}/users")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserEventResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetUserEventsByEventId(int eventId)
        {
            var userEvents = await _eventsService.GetUserEventsByEventIdAsync(eventId);
            var responses = userEvents.Select(ue => ((EventsService)_eventsService).MapToUserEventResponse(ue));
            return SuccessResponse(responses, $"Users for event ID {eventId} retrieved successfully");
        }

        /// <summary>
        /// Link a user to an event
        /// </summary>
        /// <param name="request">User-event association request</param>
        /// <returns>Created user-event association</returns>
        [HttpPost("users")]
        [ProducesResponseType(typeof(ApiResponse<UserEventResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateUserEvent([FromBody] CreateUserEventRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }

            try
            {
                var userEvent = await _eventsService.CreateUserEventAsync(request);
                var response = ((EventsService)_eventsService).MapToUserEventResponse(userEvent);
                return CreatedResponse(response, "", "User linked to event successfully");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        /// <summary>
        /// Unlink a user from an event
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="eventId">Event ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("users/{userId}/events/{eventId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteUserEvent(int userId, int eventId)
        {
            await _eventsService.DeleteUserEventAsync(userId, eventId);
            return NoContent();
        }

        /// <summary>
        /// Check if user is linked to an event
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="eventId">Event ID</param>
        /// <returns>Existence result</returns>
        [HttpGet("users/{userId}/events/{eventId}/exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UserHasEvent(int userId, int eventId)
        {
            var exists = await _eventsService.UserHasEventAsync(userId, eventId);
            return SuccessResponse(exists, $"User {userId} {(exists ? "is" : "is not")} linked to event {eventId}");
        }

        #endregion
    }
}

