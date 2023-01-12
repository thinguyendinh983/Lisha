using Lisha.Application.Common.Interfaces;

namespace Lisha.Application.Identity.Users
{
    public class UserExportDto : IDto
    {
        public string? UserName { get; set; } = default!;

        public string? Email { get; set; } = default!;

        public bool IsActive { get; set; } = default!;

        public bool EmailConfirmed { get; set; } = default!;

        public string? PhoneNumber { get; set; } = default!;
    }
}
