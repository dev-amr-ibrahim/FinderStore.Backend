using FinderStore.Backend.Domain.Entities;
using System.Security.Claims;


namespace FinderStore.Backend.Application.Common.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(ApplicationUser user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
        (string token, DateTime expires) GenerateAccessToken(ApplicationUser user, IList<string> roles);
    }
}
