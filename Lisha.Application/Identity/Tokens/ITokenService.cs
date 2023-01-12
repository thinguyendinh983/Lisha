using Lisha.Application.Common.Interfaces;

namespace Lisha.Application.Identity.Tokens
{
    public interface ITokenService : ITransientService
    {
        Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken);

        Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
    }
}
