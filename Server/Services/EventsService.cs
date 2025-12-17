using Server.Database.Interfaces;
using Server.Database.Services;
using Server.Dtos;
using Server.Interfaces;
using Server.Models.Events;
using Server.Models.Users;

namespace Server.Services
{
    public class EventsService : IEventsService
    {
        private readonly IEventTypesRepository _eventTypesRepository;
        private readonly IEventsRepository _eventsRepository;
        private readonly IUserEventsRepository _userEventsRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEventSettingsRepository _eventSettingsRepository;
        private readonly DatabaseContext _context;

        public EventsService(
            IEventTypesRepository eventTypesRepository,
            IEventsRepository eventsRepository,
            IUserEventsRepository userEventsRepository,
            IUserRepository userRepository,
            IEventSettingsRepository eventSettingsRepository,
            DatabaseContext context)
        {
            _eventTypesRepository = eventTypesRepository;
            _eventsRepository = eventsRepository;
            _userEventsRepository = userEventsRepository;
            _userRepository = userRepository;
            _eventSettingsRepository = eventSettingsRepository;
            _context = context;
        }

        // Event Types
        public async Task<IEnumerable<EventTypesModel>> GetAllEventTypesAsync()
        {
            return await _eventTypesRepository.GetAllAsync();
        }

        public async Task<EventTypesModel?> GetEventTypeByIdAsync(int id)
        {
            return await _eventTypesRepository.GetByIdAsync(id);
        }

        public async Task<EventTypesModel?> GetEventTypeByNameAsync(string name)
        {
            return await _eventTypesRepository.GetByNameAsync(name);
        }

        public async Task<EventTypesModel> CreateEventTypeAsync(CreateEventTypeRequest request)
        {
            var eventType = new EventTypesModel
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _eventTypesRepository.AddAsync(eventType);
        }

        public async Task<EventTypesModel> UpdateEventTypeAsync(int id, UpdateEventTypeRequest request)
        {
            var eventType = await _eventTypesRepository.GetByIdAsync(id);
            if (eventType == null)
            {
                throw new ArgumentException($"Event type with ID {id} not found.");
            }

            if (!string.IsNullOrEmpty(request.Name) && request.Name != eventType.Name)
            {
                if (await _eventTypesRepository.EventTypeNameExistsAsync(request.Name))
                {
                    throw new InvalidOperationException($"Event type with name '{request.Name}' already exists.");
                }
                eventType.Name = request.Name;
            }

            eventType.UpdatedAt = DateTime.UtcNow;
            await _eventTypesRepository.UpdateAsync(eventType);
            return eventType;
        }

        public async Task<bool> DeleteEventTypeAsync(int id)
        {
            var eventType = await _eventTypesRepository.GetByIdAsync(id);
            if (eventType == null)
            {
                return false;
            }

            await _eventTypesRepository.DeleteAsync(eventType);
            return true;
        }

        public async Task<bool> EventTypeNameExistsAsync(string name)
        {
            return await _eventTypesRepository.EventTypeNameExistsAsync(name);
        }

        // Events
        public async Task<IEnumerable<EventsModel>> GetAllEventsAsync(int userId)
        {
            return await _eventsRepository.FindAsync(e => e.CreatedById == userId);
        }

        public async Task<EventsModel?> GetEventByIdAsync(int id, int userId)
        {
            var eventModel = await _eventsRepository.GetEventWithTypeAsync(id);
            // Check if the current user is the creator
            if (eventModel != null && eventModel.CreatedById != userId)
            {
                return null; // Return null if user doesn't own this event
            }
            return eventModel;
        }

        public async Task<EventsModel> CreateEventAsync(CreateEventRequest request, int createdByUserId)
        {
            // Validate EventTypeId if provided
            if (request.EventTypeId.HasValue)
            {
                var eventType = await _eventTypesRepository.GetByIdAsync(request.EventTypeId.Value);
                if (eventType == null)
                {
                    throw new ArgumentException($"Event type with ID {request.EventTypeId.Value} not found.");
                }
            }

            var newEvent = new EventsModel
            {
                EventTypeId = request.EventTypeId,
                CreatedById = createdByUserId,
                Title = request.Title,
                Description = request.Description,
                CanBePostponed = request.CanBePostponed,
                IsCompleted = false, // Always default to false for new events
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _eventsRepository.AddAsync(newEvent);
        }

        public async Task<EventsModel> CreateEventWithUserAsync(CreateEventWithUserRequest request, int createdByUserId)
        {
            // Validate UserId - ensure user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {request.UserId} does not exist.");
            }

            // Validate EventTypeId if provided
            if (request.EventTypeId.HasValue)
            {
                var eventType = await _eventTypesRepository.GetByIdAsync(request.EventTypeId.Value);
                if (eventType == null)
                {
                    throw new ArgumentException($"Event type with ID {request.EventTypeId.Value} does not exist.");
                }
            }

            // Validate date logic
            if (request.StartDateTime.HasValue && request.EndDateTime.HasValue)
            {
                if (request.StartDateTime.Value >= request.EndDateTime.Value)
                {
                    throw new ArgumentException("Event start date must be before the end date.");
                }
            }

            // Validate DateTime values can be converted to UTC (prevent database errors)
            if (request.StartDateTime.HasValue)
            {
                try
                {
                    var utcStart = request.StartDateTime.Value.ToUniversalTime();
                    if (utcStart < DateTime.UtcNow.AddYears(-10) || utcStart > DateTime.UtcNow.AddYears(10))
                    {
                        throw new ArgumentException("Event start date appears to be invalid or out of reasonable range.");
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Invalid start date format: {ex.Message}");
                }
            }

            if (request.EndDateTime.HasValue)
            {
                try
                {
                    var utcEnd = request.EndDateTime.Value.ToUniversalTime();
                    if (utcEnd < DateTime.UtcNow.AddYears(-10) || utcEnd > DateTime.UtcNow.AddYears(10))
                    {
                        throw new ArgumentException("Event end date appears to be invalid or out of reasonable range.");
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Invalid end date format: {ex.Message}");
                }
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Event title is required and cannot be empty.");
            }

            // Use transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create the event first
                var newEvent = new EventsModel
                {
                    EventTypeId = request.EventTypeId,
                    CreatedById = createdByUserId,
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim(),
                    CanBePostponed = request.CanBePostponed,
                    IsCompleted = false, // Always default to false for new events
                    StartDateTime = request.StartDateTime?.ToUniversalTime(),
                    EndDateTime = request.EndDateTime?.ToUniversalTime(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdEvent = await _eventsRepository.AddAsync(newEvent);

                // Link the user to the newly created event
                var userEvent = new UserEventsModel
                {
                    UserId = request.UserId,
                    EventId = createdEvent.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userEventsRepository.AddAsync(userEvent);

                // Commit the transaction
                await transaction.CommitAsync();

                return createdEvent;
            }
            catch (Exception)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EventsModel> UpdateEventAsync(int id, UpdateEventRequest request)
        {
            var eventModel = await _eventsRepository.GetEventWithTypeAsync(id);
            if (eventModel == null)
            {
                throw new ArgumentException($"Event with ID {id} not found.");
            }

            // Validate EventTypeId if provided
            if (request.EventTypeId.HasValue)
            {
                var eventType = await _eventTypesRepository.GetByIdAsync(request.EventTypeId.Value);
                if (eventType == null)
                {
                    throw new ArgumentException($"Event type with ID {request.EventTypeId.Value} not found.");
                }
                eventModel.EventTypeId = request.EventTypeId;
            }

            if (!string.IsNullOrEmpty(request.Title))
                eventModel.Title = request.Title;

            if (request.Description != null)
                eventModel.Description = request.Description;

            if (request.CanBePostponed.HasValue)
                eventModel.CanBePostponed = request.CanBePostponed.Value;

            if (request.IsCompleted.HasValue)
                eventModel.IsCompleted = request.IsCompleted.Value;

            if (request.StartDateTime.HasValue)
                eventModel.StartDateTime = request.StartDateTime;

            if (request.EndDateTime.HasValue)
                eventModel.EndDateTime = request.EndDateTime;

            eventModel.UpdatedAt = DateTime.UtcNow;
            await _eventsRepository.UpdateAsync(eventModel);
            return eventModel;
        }

        public async Task<BulkFollowUpEventsResponse> BulkFollowUpEventsAsync(BulkFollowUpEventsRequest request, int userId)
        {
            var response = new BulkFollowUpEventsResponse();

            foreach (var eventId in request.EventIds)
            {
                try
                {
                    // Get the event
                    var eventModel = await _eventsRepository.GetEventWithTypeAsync(eventId);
                    if (eventModel == null)
                    {
                        throw new ArgumentException($"Event with ID {eventId} not found.");
                    }

                    // Check ownership
                    if (eventModel.CreatedById != userId)
                    {
                        throw new InvalidOperationException("Access denied. You can only follow up events you created.");
                    }

                    // Get user's event settings
                    var userSettings = await _eventSettingsRepository.GetByUserIdAsync(userId);
                    if (userSettings == null)
                    {
                        throw new ArgumentException($"No event settings found for user with ID {userId}. Please configure your follow-up period first.");
                    }

                    // Validate that the event has start and end dates
                    if (!eventModel.StartDateTime.HasValue || !eventModel.EndDateTime.HasValue)
                    {
                        throw new ArgumentException("Cannot perform follow-up on an event without start and end dates.");
                    }

                    // Add follow-up period days to the dates
                    eventModel.StartDateTime = eventModel.StartDateTime.Value.AddDays(userSettings.FollowUpPeriodDays);
                    eventModel.EndDateTime = eventModel.EndDateTime.Value.AddDays(userSettings.FollowUpPeriodDays);

                    // Update the event
                    eventModel.UpdatedAt = DateTime.UtcNow;
                    await _eventsRepository.UpdateAsync(eventModel);

                    var eventResponse = MapToEventResponse(eventModel);
                    response.SuccessfulEvents.Add(eventResponse);
                }
                catch (Exception ex)
                {
                    response.FailedEvents.Add(new BulkFollowUpError
                    {
                        EventId = eventId,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return response;
        }

        public async Task<BulkPostponeEventsResponse> BulkPostponeEventsAsync(BulkPostponeEventsRequest request, int userId)
        {
            var response = new BulkPostponeEventsResponse();

            foreach (var eventId in request.EventIds)
            {
                try
                {
                    // Get the event
                    var eventModel = await _eventsRepository.GetEventWithTypeAsync(eventId);
                    if (eventModel == null)
                    {
                        throw new ArgumentException($"Event with ID {eventId} not found.");
                    }

                    // Check ownership
                    if (eventModel.CreatedById != userId)
                    {
                        throw new InvalidOperationException("Access denied. You can only postpone events you created.");
                    }

                    // Validate that the event has start and end dates
                    if (!eventModel.StartDateTime.HasValue || !eventModel.EndDateTime.HasValue)
                    {
                        throw new ArgumentException("Cannot postpone an event without start and end dates.");
                    }

                    // Hard-code: Add exactly 1 day to the dates
                    eventModel.StartDateTime = eventModel.StartDateTime.Value.AddDays(1);
                    eventModel.EndDateTime = eventModel.EndDateTime.Value.AddDays(1);

                    // Update the event
                    eventModel.UpdatedAt = DateTime.UtcNow;
                    await _eventsRepository.UpdateAsync(eventModel);

                    var eventResponse = MapToEventResponse(eventModel);
                    response.SuccessfulEvents.Add(eventResponse);
                }
                catch (Exception ex)
                {
                    response.FailedEvents.Add(new BulkPostponeError
                    {
                        EventId = eventId,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return response;
        }

        /// <summary>
        /// Bulk reject events - removes user-event links. Unlike follow-up/postpone, ownership is NOT required.
        /// Any user can reject (unlink from) events they are linked to, regardless of who created the event.
        /// </summary>
        public async Task<BulkRejectEventsResponse> BulkRejectEventsAsync(BulkRejectEventsRequest request, int userId)
        {
            var response = new BulkRejectEventsResponse();

            foreach (var eventId in request.EventIds)
            {
                try
                {
                    // Check if the user is actually linked to this event
                    var userEvent = await GetUserEventAsync(userId, eventId);
                    if (userEvent == null)
                    {
                        throw new ArgumentException($"User is not linked to event with ID {eventId}.");
                    }

                    // Get event details for response
                    var eventModel = await _eventsRepository.GetByIdAsync(eventId);
                    if (eventModel == null)
                    {
                        throw new ArgumentException($"Event with ID {eventId} not found.");
                    }

                    // Remove the user-event link
                    await DeleteUserEventAsync(userId, eventId);

                    response.SuccessfulRejections.Add(new BulkRejectSuccess
                    {
                        EventId = eventId,
                        EventTitle = eventModel.Title
                    });
                }
                catch (Exception ex)
                {
                    response.FailedRejections.Add(new BulkRejectError
                    {
                        EventId = eventId,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return response;
        }

        /// <summary>
        /// Bulk complete events - marks events as completed. Available to any user, no ownership required.
        /// </summary>
        public async Task<BulkCompleteEventsResponse> BulkCompleteEventsAsync(BulkCompleteEventsRequest request, int userId)
        {
            var response = new BulkCompleteEventsResponse();

            foreach (var eventId in request.EventIds)
            {
                try
                {
                    // Get the event
                    var eventModel = await _eventsRepository.GetEventWithTypeAsync(eventId);
                    if (eventModel == null)
                    {
                        throw new ArgumentException($"Event with ID {eventId} not found.");
                    }

                    // Set completion status to true
                    eventModel.IsCompleted = true;
                    eventModel.UpdatedAt = DateTime.UtcNow;
                    await _eventsRepository.UpdateAsync(eventModel);

                    var eventResponse = MapToEventResponse(eventModel);
                    response.SuccessfulCompletions.Add(eventResponse);
                }
                catch (Exception ex)
                {
                    response.FailedCompletions.Add(new BulkCompleteError
                    {
                        EventId = eventId,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return response;
        }

        /// <summary>
        /// Toggle event completion status - available to any user, no ownership required.
        /// </summary>
        public async Task<EventsModel> ToggleEventCompletionAsync(int id, bool isCompleted)
        {
            // Get the event
            var eventModel = await _eventsRepository.GetEventWithTypeAsync(id);
            if (eventModel == null)
            {
                throw new ArgumentException($"Event with ID {id} not found.");
            }

            // Set completion status
            eventModel.IsCompleted = isCompleted;
            eventModel.UpdatedAt = DateTime.UtcNow;
            await _eventsRepository.UpdateAsync(eventModel);

            return eventModel;
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var eventModel = await _eventsRepository.GetByIdAsync(id);
            if (eventModel == null)
            {
                return false;
            }

            await _eventsRepository.DeleteAsync(eventModel);
            return true;
        }

        public async Task<IEnumerable<EventsModel>> GetEventsByTypeAsync(int eventTypeId, int userId)
        {
            var allEvents = await _eventsRepository.GetEventsByTypeAsync(eventTypeId);
            return allEvents.Where(e => e.CreatedById == userId);
        }

        public async Task<IEnumerable<EventsModel>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, int userId)
        {
            var allEvents = await _eventsRepository.GetEventsByDateRangeAsync(startDate, endDate);
            return allEvents.Where(e => e.CreatedById == userId);
        }

        public async Task<IEnumerable<EventsModel>> GetUpcomingEventsAsync(DateTime fromDate, int userId)
        {
            var allEvents = await _eventsRepository.GetUpcomingEventsAsync(fromDate);
            return allEvents.Where(e => e.CreatedById == userId);
        }

        public async Task<IEnumerable<EventsModel>> GetCompletedEventsAsync(int userId)
        {
            var allEvents = await _eventsRepository.GetCompletedEventsAsync();
            return allEvents.Where(e => e.CreatedById == userId);
        }

        public async Task<IEnumerable<EventsModel>> GetEventsByUserAsync(int userId)
        {
            return await _eventsRepository.GetEventsByUserAsync(userId);
        }

        // User Events
        public async Task<IEnumerable<UserEventsModel>> GetUserEventsByUserIdAsync(int userId)
        {
            return await _userEventsRepository.GetUserEventsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<UserEventsModel>> GetUserEventsByEventIdAsync(int eventId)
        {
            return await _userEventsRepository.GetUserEventsByEventIdAsync(eventId);
        }

        public async Task<UserEventsModel?> GetUserEventAsync(int userId, int eventId)
        {
            return await _userEventsRepository.GetUserEventAsync(userId, eventId);
        }

        public async Task<UserEventsModel> CreateUserEventAsync(CreateUserEventRequest request)
        {
            var userEvent = new UserEventsModel
            {
                UserId = request.UserId,
                EventId = request.EventId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _userEventsRepository.AddAsync(userEvent);
        }

        public async Task<bool> DeleteUserEventAsync(int userId, int eventId)
        {
            await _userEventsRepository.RemoveUserEventAsync(userId, eventId);
            return true;
        }

        public async Task<bool> UserHasEventAsync(int userId, int eventId)
        {
            return await _userEventsRepository.UserHasEventAsync(userId, eventId);
        }

        // Mapping methods
        public EventResponse MapToEventResponse(EventsModel eventModel)
        {
            return new EventResponse
            {
                Id = eventModel.Id,
                EventTypeId = eventModel.EventTypeId,
                EventTypeName = eventModel.EventType?.Name,
                CreatedById = eventModel.CreatedById,
                Title = eventModel.Title,
                Description = eventModel.Description,
                CanBePostponed = eventModel.CanBePostponed,
                IsCompleted = eventModel.IsCompleted,
                StartDateTime = eventModel.StartDateTime,
                EndDateTime = eventModel.EndDateTime,
                CreatedAt = eventModel.CreatedAt,
                UpdatedAt = eventModel.UpdatedAt
            };
        }

        public EventTypeResponse MapToEventTypeResponse(EventTypesModel eventType)
        {
            return new EventTypeResponse
            {
                Id = eventType.Id,
                Name = eventType.Name,
                CreatedAt = eventType.CreatedAt,
                UpdatedAt = eventType.UpdatedAt
            };
        }

        public UserEventResponse MapToUserEventResponse(UserEventsModel userEvent)
        {
            return new UserEventResponse
            {
                Id = userEvent.Id,
                UserId = userEvent.UserId,
                UserName = userEvent.User?.Username,
                EventId = userEvent.EventId,
                EventTitle = userEvent.Event?.Title,
                CreatedAt = userEvent.CreatedAt,
                UpdatedAt = userEvent.UpdatedAt
            };
        }
    }
}

