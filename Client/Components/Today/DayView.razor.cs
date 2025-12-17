using Microsoft.AspNetCore.Components;
using Client.Enums;
using Client.Components.Today;
using Client.Models;
using MudBlazor;
using System.Linq;
using Client.Interfaces;
using Client.Dtos;
using FluentValidation;
using Client.FluentValidation.Events;

namespace Client.Components.Today;
public partial class DayView : ComponentBase
{
    // Things needed for day view to make it's own http requests and CRUD events
    [EditorRequired]
    [Parameter] public int UserId { get; set; }
    [Parameter] public DateTime Date { get; set; } = DateTime.Today;
    [Parameter] public EventCallback<DateTime> OnDateChanged { get; set; } // EventCallback for parent for only current object


    [Parameter] public string MinimumHeight { get; set; } = "450px";
    [Parameter] public string MaximumHeight { get; set; } = "450px";
    [Parameter] public string MinimumWidth { get; set; } = "400px";
    [Parameter] public string MaximumWidth { get; set; } = "400px";
    [Parameter] public string DateTimeWidth { get; set; } = "500px";

    [Inject] private IEventsService EventsService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    public List<UserEventModel> Events { get; set; } = []; // Events will be loaded from API
    private UserEventModel? SelectedEvent { get; set; }
    private PageMode CurrentMode { get; set; } = PageMode.Read;
    private UserEventModel? _createEvent { get; set; }
    
    // Internal date tracking - don't modify Parameter directly
    private DateTime _currentDate;
    private bool _isNavigating = false; // Flag to prevent OnParametersSetAsync from resetting date during navigation
    
    // Explicit state tracking for selected events (to help Blazor detect changes)
    // Using a simple boolean field that Blazor can track easily
    private bool _hasSelectedEvents = false;

    // Form validation
    private MudForm form = null!;
    private bool isFormValid = false;
    private string[] formErrors = Array.Empty<string>();
    private CreateEventWithUserRequestValidator _createValidator = new();
    private UpdateEventRequestValidator _updateValidator = new();

    protected override async Task OnInitializedAsync()
    {
        _currentDate = Date;
        await LoadEventsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        // Sync internal date with parameter when it changes from parent
        // But don't reset if we're currently navigating (to prevent race conditions)
        if (!_isNavigating && _currentDate.Date != Date.Date)
        {
            _currentDate = Date;
        }
        await LoadEventsAsync();
    }

    private async Task LoadEventsAsync()
    {
        try
        {
            var response = await EventsService.GetEventsByUserAsync(UserId);
            if (response?.Success == true && response.Data != null)
            {
                Events = response.Data
                    .Select(MapToUserEventModel)
                    .Where(e => e.StartDateTime?.Date == _currentDate.Date) // Filter events for the current date (after converting to local time)
                    .ToList();
            }
            else
            {
                Events = new List<UserEventModel>();
            }
        }
        catch (Exception)
        {
            Events = new List<UserEventModel>();
        }
        // Reset selection count when events are reloaded
        UpdateSelectedEventCount();
        StateHasChanged();
    }
    
    private void UpdateSelectedEventCount()
    {
        _hasSelectedEvents = Events.Any(e => e.IsSelected && !e.IsCompleted);
    }

    private UserEventModel MapToUserEventModel(EventResponse eventResponse)
    {
        return new UserEventModel
        {
            Id = eventResponse.Id,
            EventType = MapEventTypeIdToEnum(eventResponse.EventTypeId),
            Title = eventResponse.Title,
            Description = eventResponse.Description ?? string.Empty,
            CanBePostponed = eventResponse.CanBePostponed,
            IsSelected = false,
            IsCompleted = eventResponse.IsCompleted,
            StartDateTime = eventResponse.StartDateTime?.ToLocalTime(),
            EndDateTime = eventResponse.EndDateTime?.ToLocalTime()
        };
    }

    private EventTypeEnums MapEventTypeIdToEnum(int? eventTypeId)
    {
        return eventTypeId switch
        {
            1 => EventTypeEnums.InternalMeeting,
            2 => EventTypeEnums.ExternalMeeting,
            3 => EventTypeEnums.WebDemo,
            4 => EventTypeEnums.HomeConference,
            5 => EventTypeEnums.AwayConference,
            6 => EventTypeEnums.SchoolVisit,
            _ => EventTypeEnums.Other
        };
    }

    public string CalculateDate() // Approved by author
    {
        return _currentDate.ToString("dddd, M/d/yyyy");
    }
    
    // Property for date display - more reliable for Blazor change detection
    private string DisplayDate => _currentDate.ToString("dddd, M/d/yyyy");
    
    // Property to check if current date is not today
    private bool IsNotToday => _currentDate.Date != DateTime.Today;

    private async Task NavigateToPreviousDay() // Approved by author
    {
        _isNavigating = true;
        _currentDate = _currentDate.AddDays(-1);
        StateHasChanged(); // Update UI immediately to show new date
        await OnDateChanged.InvokeAsync(_currentDate);
        await LoadEventsAsync();
        _isNavigating = false;
    }

    private async Task NavigateToNextDay() // Approved by author
    {
        _isNavigating = true;
        _currentDate = _currentDate.AddDays(1);
        StateHasChanged(); // Update UI immediately to show new date
        await OnDateChanged.InvokeAsync(_currentDate);
        await LoadEventsAsync();
        _isNavigating = false;
    }

    private async Task NavigateToToday()
    {
        _isNavigating = true;
        _currentDate = DateTime.Today;
        StateHasChanged(); // Update UI immediately to show new date
        await OnDateChanged.InvokeAsync(_currentDate);
        await LoadEventsAsync();
        _isNavigating = false;
    }

    private async Task HandlePostponedChanged(UserEventModel evt, bool isSelected)
    {
        // Update the event's selected state
        evt.IsSelected = isSelected;
        
        // Always recalculate and update the selection state field
        // This ensures Blazor can detect the change
        _hasSelectedEvents = Events.Any(e => e.IsSelected && !e.IsCompleted);
        
        // Always trigger state change to update UI (checkbox and buttons)
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleUndo(UserEventModel evt)
    {
        try
        {
            var request = new ToggleEventCompletionRequest
            {
                IsCompleted = false
            };

            var response = await EventsService.ToggleEventCompletionAsync(evt.Id, request);

            if (response?.Success == true && response.Data != null)
            {
                // Update the event in the list with the new data
                var index = Events.FindIndex(e => e.Id == evt.Id);
                if (index >= 0)
                {
                    Events[index] = MapToUserEventModel(response.Data);
                }

                Snackbar.Add("Event marked as incomplete successfully.", MudBlazor.Severity.Success);
                
                // Reload events to ensure we have the latest data
                await LoadEventsAsync();
            }
            else
            {
                Snackbar.Add($"Failed to undo event completion: {response?.Message ?? "Unknown error"}", MudBlazor.Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error undoing event completion: {ex.Message}", MudBlazor.Severity.Error);
        }
    }

    private void HandleEventClick(UserEventModel evt)
    {
        if (CurrentMode == PageMode.Create)
        {
            _createEvent = null;
        }
        SelectedEvent = SelectedEvent == evt ? null : evt;
        CurrentMode = SelectedEvent != null ? PageMode.Read : PageMode.Read;
        StateHasChanged();
    }

    private void HandleAddEventClick()
    {
        // Set default start time to 1 hour from current time
        var defaultStartTime = DateTime.Now.AddHours(1);
        var defaultEndTime = defaultStartTime.AddMinutes(30);

        _createEvent = new UserEventModel
        {
            Title = string.Empty,
            Description = string.Empty,
            StartDateTime = defaultStartTime,
            EndDateTime = defaultEndTime,
            EventType = EventTypeEnums.Other,
            CanBePostponed = true
        };
        SelectedEvent = null;
        CurrentMode = PageMode.Create;
        StateHasChanged();
    }

    private void HandleModeChanged(PageMode newMode)
    {
        CurrentMode = newMode;

        // If switching to edit mode, create a copy of the selected event for editing
        if (newMode == PageMode.Edit && SelectedEvent != null)
        {
            _createEvent = new UserEventModel
            {
                Id = SelectedEvent.Id,
                EventType = SelectedEvent.EventType,
                Title = SelectedEvent.Title,
                Description = SelectedEvent.Description,
                CanBePostponed = SelectedEvent.CanBePostponed,
                IsSelected = SelectedEvent.IsSelected,
                IsCompleted = SelectedEvent.IsCompleted,
                StartDateTime = SelectedEvent.StartDateTime,
                EndDateTime = SelectedEvent.EndDateTime
            };

            // Clear any previous validation errors when entering edit mode
            formErrors = Array.Empty<string>();
        }

        StateHasChanged();
    }

    private string GetEventTitle()
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
            return _createEvent.Title;
        return SelectedEvent?.Title ?? string.Empty;
    }

    private string GetEventDescription()
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
            return _createEvent.Description;
        return SelectedEvent?.Description ?? string.Empty;
    }

    private DateTime? GetEventStartDateTime()
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
            return _createEvent.StartDateTime;
        return SelectedEvent?.StartDateTime;
    }

    private DateTime? GetEventEndDateTime()
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
            return _createEvent.EndDateTime;
        return SelectedEvent?.EndDateTime;
    }

    private EventTypeEnums? GetEventType()
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
            return _createEvent.EventType;
        return SelectedEvent?.EventType;
    }

    private bool GetEventCanBePostponed()
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
            return _createEvent.CanBePostponed;
        return SelectedEvent?.CanBePostponed ?? true;
    }

    private void HandleStartDateTimeChanged(DateTime? newDateTime)
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
        {
            _createEvent.StartDateTime = newDateTime;
        }
        else if (SelectedEvent != null)
        {
            SelectedEvent.StartDateTime = newDateTime;
        }
        // Clear validation errors when user starts editing
        if (formErrors.Any())
        {
            formErrors = Array.Empty<string>();
        }
        StateHasChanged();
    }

    private void HandleEndDateTimeChanged(DateTime? newDateTime)
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
        {
            _createEvent.EndDateTime = newDateTime;
        }
        else if (SelectedEvent != null)
        {
            SelectedEvent.EndDateTime = newDateTime;
        }
        // Clear validation errors when user starts editing
        if (formErrors.Any())
        {
            formErrors = Array.Empty<string>();
        }
        StateHasChanged();
    }

    private void HandleTitleChanged(string newTitle)
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
        {
            _createEvent.Title = newTitle;
        }
        else if (SelectedEvent != null)
        {
            SelectedEvent.Title = newTitle;
        }
        // Clear validation errors when user starts editing
        if (formErrors.Any())
        {
            formErrors = Array.Empty<string>();
        }
        StateHasChanged();
    }

    private void HandleDescriptionChanged(string newDescription)
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
        {
            _createEvent.Description = newDescription;
        }
        else if (SelectedEvent != null)
        {
            SelectedEvent.Description = newDescription;
        }
        StateHasChanged();
    }

    private void HandleEventTypeChanged(EventTypeEnums? newEventType)
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
        {
            _createEvent.EventType = newEventType ?? EventTypeEnums.Other;
        }
        else if (SelectedEvent != null)
        {
            SelectedEvent.EventType = newEventType ?? EventTypeEnums.Other;
        }
        StateHasChanged();
    }

    private void HandleCanBePostponedChanged(bool canBePostponed)
    {
        if ((CurrentMode == PageMode.Create || CurrentMode == PageMode.Edit) && _createEvent != null)
        {
            _createEvent.CanBePostponed = canBePostponed;
        }
        else if (SelectedEvent != null)
        {
            SelectedEvent.CanBePostponed = canBePostponed;
        }
        StateHasChanged();
    }

    private async Task HandleSave()
    {
        if (CurrentMode == PageMode.Create && _createEvent != null)
        {
            // For create mode, use both MudForm and FluentValidation
            await form.Validate();

            if (!isFormValid)
            {
                return; // Let MudForm handle the validation display
            }

            // Additional server-side validation using FluentValidation
            // Validate with local time values (before UTC conversion) so validation matches what user sees
            var validationRequest = new CreateEventWithUserRequest
            {
                EventTypeId = MapEventTypeEnumToId(_createEvent.EventType),
                Title = _createEvent.Title,
                Description = _createEvent.Description,
                CanBePostponed = _createEvent.CanBePostponed,
                IsCompleted = _createEvent.IsCompleted,
                StartDateTime = _createEvent.StartDateTime, // Use local time for validation
                EndDateTime = _createEvent.EndDateTime, // Use local time for validation
                UserId = UserId
            };
            var validationResult = await _createValidator.ValidateAsync(validationRequest);

            if (!validationResult.IsValid)
            {
                // Handle validation errors
                formErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                StateHasChanged();
                return;
            }

            // Clear any previous errors on successful validation
            formErrors = Array.Empty<string>();
            await CreateEventWithUserAsync(_createEvent);
        }
        else if (CurrentMode == PageMode.Edit && _createEvent != null)
        {
            // For edit mode, skip MudForm validation (can be unreliable with parameter binding)
            // and rely on FluentValidation which validates the actual _createEvent data

            // Validate the edit data using FluentValidation
            // Validate with local time values (before UTC conversion) so validation matches what user sees
            var validationRequest = new UpdateEventRequest
            {
                EventTypeId = MapEventTypeEnumToId(_createEvent.EventType),
                Title = _createEvent.Title,
                Description = _createEvent.Description,
                CanBePostponed = _createEvent.CanBePostponed,
                IsCompleted = _createEvent.IsCompleted,
                StartDateTime = _createEvent.StartDateTime, // Use local time for validation
                EndDateTime = _createEvent.EndDateTime // Use local time for validation
            };
            var validationResult = await _updateValidator.ValidateAsync(validationRequest);

            if (!validationResult.IsValid)
            {
                // Handle validation errors
                formErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                StateHasChanged();
                return;
            }

            // Clear any previous errors on successful validation
            formErrors = Array.Empty<string>();
            await UpdateEventAsync(_createEvent);
        }
        StateHasChanged();
    }

    private async Task CreateEventWithUserAsync(UserEventModel eventModel)
    {
        try
        {
            var request = new CreateEventWithUserRequest
            {
                EventTypeId = MapEventTypeEnumToId(eventModel.EventType),
                Title = eventModel.Title,
                Description = eventModel.Description,
                CanBePostponed = eventModel.CanBePostponed,
                IsCompleted = eventModel.IsCompleted,
                StartDateTime = eventModel.StartDateTime?.ToUniversalTime(),
                EndDateTime = eventModel.EndDateTime?.ToUniversalTime(),
                UserId = UserId
            };

            var response = await EventsService.CreateEventWithUserAsync(request);

            if (response?.Success == true)
            {
                // Successfully created event, reload events to get the updated list
                await LoadEventsAsync();
                _createEvent = null;
                CurrentMode = PageMode.Read;
            }
            else
            {
                // TODO: Handle error - show error message to user
                Console.WriteLine($"Failed to create event: {response?.Message}");
            }
        }
        catch (Exception ex)
        {
            // TODO: Handle error - show error message to user
            Console.WriteLine($"Error creating event: {ex.Message}");
        }
    }

    private int? MapEventTypeEnumToId(EventTypeEnums eventType)
    {
        return eventType switch
        {
            EventTypeEnums.InternalMeeting => 1,
            EventTypeEnums.ExternalMeeting => 2,
            EventTypeEnums.WebDemo => 3,
            EventTypeEnums.HomeConference => 4,
            EventTypeEnums.AwayConference => 5,
            EventTypeEnums.SchoolVisit => 6,
            _ => null
        };
    }

    private CreateEventWithUserRequest CreateEventRequestFromModel(UserEventModel eventModel)
    {
        return new CreateEventWithUserRequest
        {
            EventTypeId = MapEventTypeEnumToId(eventModel.EventType),
            Title = eventModel.Title,
            Description = eventModel.Description,
            CanBePostponed = eventModel.CanBePostponed,
            IsCompleted = eventModel.IsCompleted,
            StartDateTime = eventModel.StartDateTime?.ToUniversalTime(),
            EndDateTime = eventModel.EndDateTime?.ToUniversalTime(),
            UserId = UserId
        };
    }

    private UpdateEventRequest CreateUpdateEventRequestFromModel(UserEventModel eventModel)
    {
        return new UpdateEventRequest
        {
            EventTypeId = MapEventTypeEnumToId(eventModel.EventType),
            Title = eventModel.Title,
            Description = eventModel.Description,
            CanBePostponed = eventModel.CanBePostponed,
            IsCompleted = eventModel.IsCompleted,
            StartDateTime = eventModel.StartDateTime?.ToUniversalTime(),
            EndDateTime = eventModel.EndDateTime?.ToUniversalTime()
        };
    }

    private async Task UpdateEventAsync(UserEventModel eventModel)
    {
        try
        {
            var request = CreateUpdateEventRequestFromModel(eventModel);
            var response = await EventsService.UpdateEventAsync(eventModel.Id, request);

            if (response?.Success == true && response.Data != null)
            {
                // Update the event in the list with the new data
                var index = Events.FindIndex(e => e.Id == eventModel.Id);
                if (index >= 0)
                {
                    Events[index] = MapToUserEventModel(response.Data);
                }

                // Switch back to read mode
                SelectedEvent = null;
                CurrentMode = PageMode.Read;
            }
            else
            {
                // TODO: Handle error - show error message to user
                Console.WriteLine($"Failed to update event: {response?.Message}");
            }
        }
        catch (Exception ex)
        {
            // TODO: Handle error - show error message to user
            Console.WriteLine($"Error updating event: {ex.Message}");
        }
    }

    private void HandleCancel()
    {
        if (CurrentMode == PageMode.Create)
        {
            _createEvent = null;
            CurrentMode = PageMode.Read;
        }
        else if (CurrentMode == PageMode.Edit)
        {
            _createEvent = null; // Clear the edit copy
            CurrentMode = PageMode.Read;
        }
        StateHasChanged();
    }

    private int GetSelectedEventCount()
    {
        return Events.Count(e => e.IsSelected && !e.IsCompleted);
    }

    private IEnumerable<UserEventModel> GetUncompletedEvents()
    {
        return Events.Where(e => !e.IsCompleted).OrderBy(e => e.StartDateTime ?? DateTime.MaxValue);
    }

    private IEnumerable<UserEventModel> GetCompletedEvents()
    {
        return Events.Where(e => e.IsCompleted).OrderBy(e => e.StartDateTime ?? DateTime.MaxValue);
    }

    private async Task HandleBulkPostpone()
    {
        var selectedEvents = Events.Where(e => e.IsSelected && !e.IsCompleted && e.CanBePostponed).ToList();
        
        if (selectedEvents.Count == 0)
        {
            Snackbar.Add("Please select at least one event that can be postponed.", MudBlazor.Severity.Warning);
            return;
        }

        try
        {
            var request = new BulkPostponeEventsRequest
            {
                EventIds = selectedEvents.Select(e => e.Id).ToList()
            };

            var response = await EventsService.BulkPostponeEventsAsync(request);

            if (response?.Success == true && response.Data != null)
            {
                var result = response.Data;
                var successCount = result.SuccessfulEvents.Count;
                var failCount = result.FailedEvents.Count;

                // Update successful events in the list
                foreach (var successfulEvent in result.SuccessfulEvents)
                {
                    var index = Events.FindIndex(e => e.Id == successfulEvent.Id);
                    if (index >= 0)
                    {
                        Events[index] = MapToUserEventModel(successfulEvent);
                        Events[index].IsSelected = false; // Clear selection
                    }
                }
                // Update selection count after clearing selections
                UpdateSelectedEventCount();

                // Show feedback
                if (failCount == 0)
                {
                    Snackbar.Add($"Successfully postponed {successCount} event(s).", MudBlazor.Severity.Success);
                }
                else
                {
                    Snackbar.Add($"Postponed {successCount} event(s). {failCount} event(s) failed.", MudBlazor.Severity.Warning);
                    // Log failed events
                    foreach (var failedEvent in result.FailedEvents)
                    {
                        Snackbar.Add($"Event {failedEvent.EventId}: {failedEvent.ErrorMessage}", MudBlazor.Severity.Error);
                    }
                }

                // Reload events to ensure we have the latest data
                await LoadEventsAsync();
            }
            else
            {
                Snackbar.Add($"Failed to postpone events: {response?.Message ?? "Unknown error"}", MudBlazor.Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error postponing events: {ex.Message}", MudBlazor.Severity.Error);
        }
    }

    private async Task HandleBulkFollowUp()
    {
        var selectedEvents = Events.Where(e => e.IsSelected && !e.IsCompleted).ToList();
        
        if (selectedEvents.Count == 0)
        {
            Snackbar.Add("Please select at least one event to follow up.", MudBlazor.Severity.Warning);
            return;
        }

        try
        {
            var request = new BulkFollowUpEventsRequest
            {
                EventIds = selectedEvents.Select(e => e.Id).ToList()
            };

            var response = await EventsService.BulkFollowUpEventsAsync(request);

            if (response?.Success == true && response.Data != null)
            {
                var result = response.Data;
                var successCount = result.SuccessfulEvents.Count;
                var failCount = result.FailedEvents.Count;

                // Update successful events in the list
                foreach (var successfulEvent in result.SuccessfulEvents)
                {
                    var index = Events.FindIndex(e => e.Id == successfulEvent.Id);
                    if (index >= 0)
                    {
                        Events[index] = MapToUserEventModel(successfulEvent);
                        Events[index].IsSelected = false; // Clear selection
                    }
                }
                // Update selection count after clearing selections
                UpdateSelectedEventCount();

                // Show feedback
                if (failCount == 0)
                {
                    Snackbar.Add($"Successfully followed up {successCount} event(s).", MudBlazor.Severity.Success);
                }
                else
                {
                    Snackbar.Add($"Followed up {successCount} event(s). {failCount} event(s) failed.", MudBlazor.Severity.Warning);
                    // Log failed events
                    foreach (var failedEvent in result.FailedEvents)
                    {
                        Snackbar.Add($"Event {failedEvent.EventId}: {failedEvent.ErrorMessage}", MudBlazor.Severity.Error);
                    }
                }

                // Reload events to ensure we have the latest data
                await LoadEventsAsync();
            }
            else
            {
                Snackbar.Add($"Failed to follow up events: {response?.Message ?? "Unknown error"}", MudBlazor.Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error following up events: {ex.Message}", MudBlazor.Severity.Error);
        }
    }

    private async Task HandleBulkComplete()
    {
        var selectedEvents = Events.Where(e => e.IsSelected && !e.IsCompleted).ToList();
        
        if (selectedEvents.Count == 0)
        {
            Snackbar.Add("Please select at least one event to complete.", MudBlazor.Severity.Warning);
            return;
        }

        try
        {
            var request = new BulkCompleteEventsRequest
            {
                EventIds = selectedEvents.Select(e => e.Id).ToList()
            };

            var response = await EventsService.BulkCompleteEventsAsync(request);

            if (response?.Success == true && response.Data != null)
            {
                var result = response.Data;
                var successCount = result.SuccessfulCompletions.Count;
                var failCount = result.FailedCompletions.Count;

                // Update successful events in the list
                foreach (var successfulEvent in result.SuccessfulCompletions)
                {
                    var index = Events.FindIndex(e => e.Id == successfulEvent.Id);
                    if (index >= 0)
                    {
                        Events[index] = MapToUserEventModel(successfulEvent);
                        Events[index].IsSelected = false; // Clear selection
                    }
                }
                // Update selection count after clearing selections
                UpdateSelectedEventCount();

                // Show feedback
                if (failCount == 0)
                {
                    Snackbar.Add($"Successfully completed {successCount} event(s).", MudBlazor.Severity.Success);
                }
                else
                {
                    Snackbar.Add($"Completed {successCount} event(s). {failCount} event(s) failed.", MudBlazor.Severity.Warning);
                    // Log failed events
                    foreach (var failedEvent in result.FailedCompletions)
                    {
                        Snackbar.Add($"Event {failedEvent.EventId}: {failedEvent.ErrorMessage}", MudBlazor.Severity.Error);
                    }
                }

                // Reload events to ensure we have the latest data
                await LoadEventsAsync();
            }
            else
            {
                Snackbar.Add($"Failed to complete events: {response?.Message ?? "Unknown error"}", MudBlazor.Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error completing events: {ex.Message}", MudBlazor.Severity.Error);
        }
    }

    private async Task HandleBulkReject()
    {
        var selectedEvents = Events.Where(e => e.IsSelected && !e.IsCompleted).ToList();
        
        if (selectedEvents.Count == 0)
        {
            Snackbar.Add("Please select at least one event to reject.", MudBlazor.Severity.Warning);
            return;
        }

        try
        {
            var request = new BulkRejectEventsRequest
            {
                EventIds = selectedEvents.Select(e => e.Id).ToList()
            };

            var response = await EventsService.BulkRejectEventsAsync(request);

            if (response?.Success == true && response.Data != null)
            {
                var result = response.Data;
                var successCount = result.SuccessfulRejections.Count;
                var failCount = result.FailedRejections.Count;

                // Remove successfully rejected events from the list (they're unlinked from the user)
                var rejectedEventIds = result.SuccessfulRejections.Select(r => r.EventId).ToHashSet();
                Events.RemoveAll(e => rejectedEventIds.Contains(e.Id));
                
                // Update selection count after removing events
                UpdateSelectedEventCount();

                // Show feedback
                if (failCount == 0)
                {
                    Snackbar.Add($"Successfully rejected {successCount} event(s).", MudBlazor.Severity.Success);
                }
                else
                {
                    Snackbar.Add($"Rejected {successCount} event(s). {failCount} event(s) failed.", MudBlazor.Severity.Warning);
                    // Log failed events
                    foreach (var failedEvent in result.FailedRejections)
                    {
                        Snackbar.Add($"Event {failedEvent.EventId}: {failedEvent.ErrorMessage}", MudBlazor.Severity.Error);
                    }
                }

                // Reload events to ensure we have the latest data
                await LoadEventsAsync();
            }
            else
            {
                Snackbar.Add($"Failed to reject events: {response?.Message ?? "Unknown error"}", MudBlazor.Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error rejecting events: {ex.Message}", MudBlazor.Severity.Error);
        }
    }
}
