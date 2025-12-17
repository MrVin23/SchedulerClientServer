using Server.Models.UserPermissions;

namespace Server.Models.Users
{
    // TODO: Add required fields
    public class User : ModelBase
    {
        public string? Username { get; set; } // Required
        public string? FirstName { get; set; } // Optional
        public string? LastName { get; set; } // Optional
        public string? Email { get; set; } // Required
        public string? Password { get; set; } // Stored as BCrypt hash in database
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}