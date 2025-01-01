using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Lisha.Shared.Authorization
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetEmail(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.Email);

        //public static string? GetTenant(this ClaimsPrincipal principal)
        //    => principal.FindFirstValue(AppClaims.Tenant);

        public static string? GetFullName(this ClaimsPrincipal principal)
            => principal?.FindFirst(AppClaims.Fullname)?.Value;

        public static string? GetFirstName(this ClaimsPrincipal principal)
            => principal?.FindFirst(ClaimTypes.Name)?.Value;

        public static string? GetSurname(this ClaimsPrincipal principal)
            => principal?.FindFirst(ClaimTypes.Surname)?.Value;

        public static string? GetPhoneNumber(this ClaimsPrincipal principal)
            => principal.FindFirstValue(ClaimTypes.MobilePhone);

        public static string? GetUserId(this ClaimsPrincipal principal)
           => principal.FindFirstValue(ClaimTypes.NameIdentifier);

        public static Uri? GetImageUrl(this ClaimsPrincipal principal)
        {
            var imageUrl = principal.FindFirstValue(AppClaims.ImageUrl);
            return Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ? uri : null;
        }

        //public static DateTimeOffset GetExpiration(this ClaimsPrincipal principal) =>
        //    DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(
        //        principal.FindFirstValue(AppClaims.Expiration)));

        private static string? FindFirstValue(this ClaimsPrincipal principal, string claimType) =>
            principal is null
                ? throw new ArgumentNullException(nameof(principal))
                : principal.FindFirst(claimType)?.Value;
    }
}
