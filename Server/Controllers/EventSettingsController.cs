using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Controllers;
using Server.Dtos;
using Server.Interfaces;
using Server.Services;
using Server.Utils;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Admin")]
    public class EventSettingsController : BaseController
    {
        private readonly IEventSettingsService _eventSettingsService;
        private readonly ILogger<EventSettingsController> _logger;

        public EventSettingsController(IEventSettingsService eventSettingsService, ILogger<EventSettingsController> logger)
        {
            _eventSettingsService = eventSettingsService;
            _logger = logger;
        }

        /// <summary>
        /// Get event settings for the current user
        /// </summary>
        /// <returns>User's event settings</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<EventSettingsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetMySettings()
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == 0)
            {
                return BadRequestResponse("User authentication required");
            }

            var settings = await _eventSettingsService.GetByUserIdAsync(currentUserId);
            if (settings == null)
            {
                return NotFoundResponse("Event settings not found for current user");
            }

            var response = ((EventSettingsService)_eventSettingsService).MapToEventSettingsResponse(settings);
            return SuccessResponse(response, "Event settings retrieved successfully");
        }

        /// <summary>
        /// Get event settings for a specific user (admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User's event settings</returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(ApiResponse<EventSettingsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetSettingsByUserId(int userId)
        {
            var settings = await _eventSettingsService.GetByUserIdAsync(userId);
            if (settings == null)
            {
                return NotFoundResponse($"Event settings not found for user with ID {userId}");
            }

            var response = ((EventSettingsService)_eventSettingsService).MapToEventSettingsResponse(settings);
            return SuccessResponse(response, "Event settings retrieved successfully");
        }

        /// <summary>
        /// Create event settings for the current user
        /// </summary>
        /// <param name="request">Event settings creation request</param>
        /// <returns>Created event settings</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EventSettingsResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateMySettings([FromBody] CreateEventSettingsRequest request)
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
                    return BadRequestResponse("User authentication required to create event settings");
                }

                var settingsModel = await _eventSettingsService.CreateEventSettingsAsync(request, currentUserId);
                var response = ((EventSettingsService)_eventSettingsService).MapToEventSettingsResponse(settingsModel);
                var location = Url.Action(nameof(GetMySettings)) ?? string.Empty;
                return CreatedResponse(response, location, "Event settings created successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        /// <summary>
        /// Update event settings for the current user
        /// </summary>
        /// <param name="request">Event settings update request</param>
        /// <returns>Updated event settings</returns>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<EventSettingsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateMySettings([FromBody] UpdateEventSettingsRequest request)
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
                    return BadRequestResponse("User authentication required to update event settings");
                }

                var settingsModel = await _eventSettingsService.UpdateEventSettingsAsync(currentUserId, request);
                var response = ((EventSettingsService)_eventSettingsService).MapToEventSettingsResponse(settingsModel);
                return SuccessResponse(response, "Event settings updated successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        /// <summary>
        /// Delete event settings for the current user
        /// </summary>
        /// <returns>Success status</returns>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteMySettings()
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == 0)
            {
                return BadRequestResponse("User authentication required to delete event settings");
            }

            var deleted = await _eventSettingsService.DeleteEventSettingsAsync(currentUserId);
            if (!deleted)
            {
                return NotFoundResponse("Event settings not found for current user");
            }

            return SuccessResponse(deleted, "Event settings deleted successfully");
        }

        /// <summary>
        /// Check if current user has event settings
        /// </summary>
        /// <returns>Whether user has settings</returns>
        [HttpGet("exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> HasSettings()
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == 0)
            {
                return BadRequestResponse("User authentication required");
            }

            var hasSettings = await _eventSettingsService.UserHasSettingsAsync(currentUserId);
            return SuccessResponse(hasSettings, "Settings existence checked successfully");
        }
    }
}
