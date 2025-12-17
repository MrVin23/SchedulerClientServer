using FluentValidation;
using Client.Dtos;

namespace Client.FluentValidation.Events
{
    public class CreateEventWithUserRequestValidator : AbstractValidator<CreateEventWithUserRequest>
    {
        private string GetCurrentTimeInfo()
        {
            return $"Current time: {FormatDateTime(DateTime.Now)}";
        }

        private string FormatDateTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "null";
            
            // Format as local time (what user sees) with timezone indicator
            var dt = dateTime.Value;
            if (dt.Kind == DateTimeKind.Utc)
            {
                dt = dt.ToLocalTime();
            }
            return dt.ToString("yyyy-MM-dd HH:mm:ss") + " (Local)";
        }

        public CreateEventWithUserRequestValidator()
        {
            // Title is required and cannot be empty or whitespace
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .NotNull().WithMessage("Title cannot be null")
                .MinimumLength(1).WithMessage("Title must contain at least 1 character")
                .MaximumLength(200).WithMessage(x => $"Title cannot exceed 200 characters. Your title length: {x.Title?.Length ?? 0}");

            // Start date/time cannot be in the past
            RuleFor(x => x.StartDateTime)
                .NotEmpty().WithMessage("Start date and time is required")
                .Must(BeInTheFuture).WithMessage(x => $"Start date and time cannot be in the past. You selected: {FormatDateTime(x.StartDateTime)}. {GetCurrentTimeInfo()}");

            // End date/time validation
            RuleFor(x => x.EndDateTime)
                .NotEmpty().WithMessage("End date and time is required")
                .Must(BeInTheFuture).WithMessage(x => $"End date and time cannot be in the past. You selected: {FormatDateTime(x.EndDateTime)}. {GetCurrentTimeInfo()}")
                .GreaterThan(x => x.StartDateTime).WithMessage(x => $"End time must be after start time. Start: {FormatDateTime(x.StartDateTime)}, End: {FormatDateTime(x.EndDateTime)}")
                .Must((model, endDateTime) => BeWithin8Hours(model.StartDateTime, endDateTime))
                .WithMessage(x => $"End time cannot be more than 8 hours after start time. Start: {FormatDateTime(x.StartDateTime)}, End: {FormatDateTime(x.EndDateTime)}, Difference: {(x.EndDateTime - x.StartDateTime)?.TotalHours.ToString("F1") ?? "N/A"} hours");

            // UserId must be greater than 0
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage(x => $"Valid user ID is required. You provided: {x.UserId}");

            // Optional description has maximum length
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage(x => $"Description cannot exceed 1000 characters. Your description length: {x.Description?.Length ?? 0}")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // EventTypeId validation (optional field)
            RuleFor(x => x.EventTypeId)
                .GreaterThan(0).WithMessage(x => $"Invalid event type selected. You selected: {x.EventTypeId}")
                .When(x => x.EventTypeId.HasValue);
        }

        private bool BeInTheFuture(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return false;

            return dateTime.Value > DateTime.Now;
        }

        private bool BeWithin8Hours(DateTime? startDateTime, DateTime? endDateTime)
        {
            if (!startDateTime.HasValue || !endDateTime.HasValue)
                return false;

            var timeDifference = endDateTime.Value - startDateTime.Value;
            return timeDifference.TotalHours <= 8;
        }
    }

    public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
    {
        private string GetCurrentTimeInfo()
        {
            return $"Current time: {FormatDateTime(DateTime.Now)}";
        }

        private string FormatDateTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "null";
            
            // Format as local time (what user sees) with timezone indicator
            var dt = dateTime.Value;
            if (dt.Kind == DateTimeKind.Utc)
            {
                dt = dt.ToLocalTime();
            }
            return dt.ToString("yyyy-MM-dd HH:mm:ss") + " (Local)";
        }

        public UpdateEventRequestValidator()
        {
            // Title validation (only if provided)
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty")
                .MinimumLength(1).WithMessage("Title must contain at least 1 character")
                .MaximumLength(200).WithMessage(x => $"Title cannot exceed 200 characters. Your title length: {x.Title?.Length ?? 0}")
                .When(x => !string.IsNullOrEmpty(x.Title));

            // Start date/time validation (only if provided)
            RuleFor(x => x.StartDateTime)
                .Must(BeInTheFuture).WithMessage(x => $"Start date and time cannot be in the past. You selected: {FormatDateTime(x.StartDateTime)}. {GetCurrentTimeInfo()}")
                .When(x => x.StartDateTime.HasValue);

            // End date/time validation
            RuleFor(x => x.EndDateTime)
                .Must(BeInTheFuture).WithMessage(x => $"End date and time cannot be in the past. You selected: {FormatDateTime(x.EndDateTime)}. {GetCurrentTimeInfo()}")
                .Must((model, endDateTime) => BeAfterStartTime(model.StartDateTime, endDateTime))
                .WithMessage(x => $"End time must be after start time. Start: {FormatDateTime(x.StartDateTime)}, End: {FormatDateTime(x.EndDateTime)}")
                .Must((model, endDateTime) => BeWithin8Hours(model.StartDateTime, endDateTime))
                .WithMessage(x => $"End time cannot be more than 8 hours after start time. Start: {FormatDateTime(x.StartDateTime)}, End: {FormatDateTime(x.EndDateTime)}, Difference: {(x.EndDateTime - x.StartDateTime)?.TotalHours.ToString("F1") ?? "N/A"} hours")
                .When(x => x.EndDateTime.HasValue);

            // Optional description has maximum length
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage(x => $"Description cannot exceed 1000 characters. Your description length: {x.Description?.Length ?? 0}")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // EventTypeId validation (optional field)
            RuleFor(x => x.EventTypeId)
                .GreaterThan(0).WithMessage(x => $"Invalid event type selected. You selected: {x.EventTypeId}")
                .When(x => x.EventTypeId.HasValue);
        }

        private bool BeInTheFuture(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return true; // Allow null values for updates

            return dateTime.Value > DateTime.Now;
        }

        private bool BeAfterStartTime(DateTime? startDateTime, DateTime? endDateTime)
        {
            if (!startDateTime.HasValue || !endDateTime.HasValue)
                return true; // Allow null values for updates

            return endDateTime.Value > startDateTime.Value;
        }

        private bool BeWithin8Hours(DateTime? startDateTime, DateTime? endDateTime)
        {
            if (!startDateTime.HasValue || !endDateTime.HasValue)
                return true; // Allow null values for updates

            var timeDifference = endDateTime.Value - startDateTime.Value;
            return timeDifference.TotalHours <= 8;
        }
    }
}
