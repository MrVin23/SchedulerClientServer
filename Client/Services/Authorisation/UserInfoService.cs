using Client.Dtos;
using Client.Interfaces.Authorisation;

namespace Client.Services.Authorisation
{
    public class UserInfoService : IUserInfoService
    {
        private readonly ISecureStorageService _secureStorage;
        private const string CurrentUserKey = "currentUser";

        public UserInfoService(ISecureStorageService secureStorage)
        {
            _secureStorage = secureStorage;
        }

        /// <summary>
        /// Get the current user's first name from session storage
        /// </summary>
        public async Task<string?> GetFirstNameAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.FirstName;
        }

        /// <summary>
        /// Get the current user's last name from session storage
        /// </summary>
        public async Task<string?> GetLastNameAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.LastName;
        }

        /// <summary>
        /// Get the current user's username from session storage
        /// </summary>
        public async Task<string?> GetUsernameAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.Username;
        }

        /// <summary>
        /// Get the current user's email from session storage
        /// </summary>
        public async Task<string?> GetEmailAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.Email;
        }

        /// <summary>
        /// Get the full current user object from session storage
        /// </summary>
        public async Task<LoginResponse?> GetCurrentUserAsync()
        {
            return await _secureStorage.GetAsync<LoginResponse>(CurrentUserKey);
        }

        /// <summary>
        /// Check if a user is currently logged in
        /// </summary>
        public async Task<bool> IsUserLoggedInAsync()
        {
            var user = await GetCurrentUserAsync();
            return user != null;
        }
    }
}