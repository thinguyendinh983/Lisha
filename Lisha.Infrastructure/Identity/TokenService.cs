using Lisha.Application.Common.Exceptions;
using Lisha.Application.Common.Interfaces;
using Lisha.Application.Identity.Tokens;
using Lisha.Infrastructure.Auth;
using Lisha.Infrastructure.Auth.Jwt;
using Lisha.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Lisha.Infrastructure.Identity
{
    internal class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<TokenService> _localizer;
        private readonly SecuritySettings _securitySettings;
        private readonly JwtSettings _jwtSettings;
        private readonly IDateTime _dateTime;

        public TokenService(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtSettings,
            IStringLocalizer<TokenService> localizer,
            IOptions<SecuritySettings> securitySettings,
            IDateTime dateTime)
        {
            _userManager = userManager;
            _localizer = localizer;
            _jwtSettings = jwtSettings.Value;
            _securitySettings = securitySettings.Value;
            _dateTime = dateTime;
        }

        public async Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email.Trim().Normalize());
            if (user is null)
            {
                throw new UnauthorizedException(_localizer["auth.failed"]);
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedException(_localizer["identity.usernotactive"]);
            }

            if (_securitySettings.RequireConfirmedAccount && !user.EmailConfirmed)
            {
                throw new UnauthorizedException(_localizer["identity.emailnotconfirmed"]);
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedException(_localizer["identity.invalidcredentials"]);
            }

            return await GenerateTokensAndUpdateUser(user, ipAddress);
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
        {
            var userPrincipal = GetPrincipalFromExpiredToken(request.Token);
            string? userEmail = userPrincipal.GetEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user is null)
            {
                throw new UnauthorizedException(_localizer["auth.failed"]);
            }

            if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= _dateTime.Now)
            {
                throw new UnauthorizedException(_localizer["identity.invalidrefreshtoken"]);
            }

            return await GenerateTokensAndUpdateUser(user, ipAddress);
        }

        private async Task<TokenResponse> GenerateTokensAndUpdateUser(ApplicationUser user, string ipAddress)
        {
            string token = GenerateJwt(user, ipAddress);

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = _dateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

            await _userManager.UpdateAsync(user);

            return new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
        }

        private string GenerateJwt(ApplicationUser user, string ipAddress) =>
            GenerateEncryptedToken(GetSigningCredentials(), GetClaims(user, ipAddress));

        private IEnumerable<Claim> GetClaims(ApplicationUser user, string ipAddress) =>
            new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(AppClaims.Fullname, $"{user.FirstName} {user.LastName}"),
                new(ClaimTypes.Name, user.FirstName ?? string.Empty),
                new(ClaimTypes.Surname, user.LastName ?? string.Empty),
                new(AppClaims.IpAddress, ipAddress),
                new(AppClaims.ImageUrl, user.ImageUrl ?? string.Empty),
                new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
            };

        private string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
               claims: claims,
               expires: _dateTime.Now.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
               signingCredentials: signingCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            if (string.IsNullOrEmpty(_jwtSettings.Key))
            {
                throw new InvalidOperationException("No Key defined in JwtSettings config.");
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UnauthorizedException(_localizer["identity.invalidtoken"]);
            }

            return principal;
        }

        private SigningCredentials GetSigningCredentials()
        {
            if (string.IsNullOrEmpty(_jwtSettings.Key))
            {
                throw new InvalidOperationException("No Key defined in JwtSettings config.");
            }

            byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }
    }
}
