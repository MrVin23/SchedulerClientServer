using Client.Dtos;

namespace Client.Interfaces.Authorisation
{
    public interface IUserInfoService
    {
        /// <summary>
        /// Get the current user's first name from session storage
        /// </summary>
        Task<string?> GetFirstNameAsync();

        /// <summary>
        /// Get the current user's last name from session storage
        /// </summary>
        Task<string?> GetLastNameAsync();

        /// <summary>
        /// Get the current user's username from session storage
        /// </summary>
        Task<string?> GetUsernameAsync();

        /// <summary>
        /// Get the current user's email from session storage
        /// </summary>
        Task<string?> GetEmailAsync();

        /// <summary>
        /// Get the full current user object from session storage
        /// </summary>
        Task<LoginResponse?> GetCurrentUserAsync();

        /// <summary>
        /// Check if a user is currently logged in
        /// </summary>
        Task<bool> IsUserLoggedInAsync();
    }
}

