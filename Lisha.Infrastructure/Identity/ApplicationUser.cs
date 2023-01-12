using Microsoft.AspNetCore.Identity;

namespace Lisha.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public DateTime? LastLoginOn { get; set; }
        public string? ClientName { get; set; }
        public string? ClientIpAddress { get; set; }
        public string? ObjectId { get; set; }
    }
}
