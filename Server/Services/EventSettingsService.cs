using Server.Database.Interfaces;
using Server.Database.Services;
using Server.Dtos;
using Server.Interfaces;
using Server.Models.Events;

namespace Server.Services
{
    public class EventSettingsService : IEventSettingsService
    {
        private readonly IEventSettingsRepository _eventSettingsRepository;
        private readonly IUserRepository _userRepository;
        private readonly DatabaseContext _context;

        public EventSettingsService(
            IEventSettingsRepository eventSettingsRepository,
            IUserRepository userRepository,
            DatabaseContext context)
        {
            _eventSettingsRepository = eventSettingsRepository;
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<EventSettingsModel?> GetByUserIdAsync(int userId)
        {
            return await _eventSettingsRepository.GetByUserIdAsync(userId);
        }

        public async Task<EventSettingsModel> CreateEventSettingsAsync(CreateEventSettingsRequest request, int userId)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }

            // Check if user already has settings
            if (await _eventSettingsRepository.UserHasSettingsAsync(userId))
            {
                throw new ArgumentException($"User with ID {userId} already has event settings");
            }

            // Validate follow-up period days (should be positive)
            if (request.FollowUpPeriodDays <= 0)
            {
                throw new ArgumentException("FollowUpPeriodDays must be greater than 0");
            }

            var eventSettings = new EventSettingsModel
            {
                UserId = userId,
                FollowUpPeriodDays = request.FollowUpPeriodDays
            };

            return await _eventSettingsRepository.AddAsync(eventSettings);
        }

        public async Task<EventSettingsModel> UpdateEventSettingsAsync(int userId, UpdateEventSettingsRequest request)
        {
            // Get existing settings
            var existingSettings = await _eventSettingsRepository.GetByUserIdAsync(userId);
            if (existingSettings == null)
            {
                throw new ArgumentException($"No event settings found for user with ID {userId}");
            }

            // Update properties if provided
            if (request.FollowUpPeriodDays.HasValue)
            {
                if (request.FollowUpPeriodDays.Value <= 0)
                {
                    throw new ArgumentException("FollowUpPeriodDays must be greater than 0");
                }
                existingSettings.FollowUpPeriodDays = request.FollowUpPeriodDays.Value;
            }

            await _eventSettingsRepository.UpdateAsync(existingSettings);
            return existingSettings;
        }

        public async Task<bool> DeleteEventSettingsAsync(int userId)
        {
            var existingSettings = await _eventSettingsRepository.GetByUserIdAsync(userId);
            if (existingSettings == null)
            {
                return false;
            }

            await _eventSettingsRepository.DeleteAsync(existingSettings);
            return true;
        }

        public async Task<bool> UserHasSettingsAsync(int userId)
        {
            return await _eventSettingsRepository.UserHasSettingsAsync(userId);
        }

        // Helper method to map to response DTO
        public EventSettingsResponse MapToEventSettingsResponse(EventSettingsModel model)
        {
            return new EventSettingsResponse
            {
                Id = model.Id,
                UserId = model.UserId,
                UserName = model.User?.Username,
                FollowUpPeriodDays = model.FollowUpPeriodDays,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt
            };
        }
    }
}
