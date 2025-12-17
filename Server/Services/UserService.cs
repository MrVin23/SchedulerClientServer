using Server.Database.Interfaces;
using Server.Dtos;
using Server.Interfaces;
using Server.Models.Users;
using Server.Utils;

namespace Server.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly DuplicateChecker _duplicateChecker;

        public UserService(IUserRepository userRepository, IUserRoleRepository userRoleRepository, DuplicateChecker duplicateChecker)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _duplicateChecker = duplicateChecker;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            // Create a temporary user object for duplicate checking
            var userToCheck = new User
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = PasswordHelper.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Check for duplicates using DuplicateChecker
            var duplicateResults = await _duplicateChecker.CheckForDuplicate(userToCheck,
                u => u.Username!,
                u => u.Email!
            );

            // Check if any duplicates were found
            var duplicateFields = duplicateResults
                .Where(r => r.IsDuplicate)
                .Select(r => r.DuplicateField)
                .ToList();

            if (duplicateFields.Any())
            {
                var fieldList = string.Join(" and ", duplicateFields);
                throw new InvalidOperationException($"The following fields already exist: {fieldList}");
            }

            return await _userRepository.AddAsync(userToCheck);
        }

        public async Task<User> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {id} not found.");
            }

            // Create temporary user with updated fields for duplicate checking
            var tempUser = new User
            {
                Username = !string.IsNullOrEmpty(request.Username) ? request.Username : user.Username,
                Email = !string.IsNullOrEmpty(request.Email) ? request.Email : user.Email,
                FirstName = !string.IsNullOrEmpty(request.FirstName) ? request.FirstName : user.FirstName,
                LastName = !string.IsNullOrEmpty(request.LastName) ? request.LastName : user.LastName
            };

            // Check for duplicates only if username or email is being changed
            var propertiesToCheck = new List<System.Linq.Expressions.Expression<Func<User, object>>>();
            
            if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
            {
                propertiesToCheck.Add(u => u.Username!);
            }

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                propertiesToCheck.Add(u => u.Email!);
            }

            if (propertiesToCheck.Any())
            {
                var duplicateResults = await _duplicateChecker.CheckForDuplicate(tempUser, propertiesToCheck.ToArray());
                
                // Exclude the current user from duplicate check by filtering out matches with the same ID
                // This requires getting all duplicates and checking if they're the current user
                // For now, we'll use the existing repository method as a fallback
                if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
                {
                    if (await _userRepository.UsernameExistsAsync(request.Username))
                    {
                        throw new InvalidOperationException($"Username '{request.Username}' already exists.");
                    }
                }

                if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
                {
                    if (await _userRepository.EmailExistsAsync(request.Email))
                    {
                        throw new InvalidOperationException($"Email '{request.Email}' already exists.");
                    }
                }
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Username))
                user.Username = request.Username;

            if (!string.IsNullOrEmpty(request.Email))
                user.Email = request.Email;

            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.Password))
                user.Password = PasswordHelper.HashPassword(request.Password);

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(user);
            return true;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _userRepository.UsernameExistsAsync(username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId)
        {
            return await _userRepository.GetUsersByRoleAsync(roleId);
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                return null;
            }

            // Verify the password using BCrypt
            if (!PasswordHelper.VerifyPassword(password, user.Password))
            {
                return null;
            }

            return user;
        }

        public UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
            };
        }
    }
}
