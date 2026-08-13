using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FinderStore.Backend.Application.Features.Auth.Commands
{
    public record RefreshTokenCommand : IRequest<AuthResponse>
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public RefreshTokenCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Validate the expired token
            var principal = _tokenService.ValidateToken(request.AccessToken);
            if (principal == null)
                throw new UnauthorizedAccessException("Invalid access token");

            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Invalid token claims");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated");

            // Here you should validate the refresh token against stored value
            // For now, we just generate new tokens

            var roles = await _userManager.GetRolesAsync(user);
            var (newToken, expires) = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            return new AuthResponse
            {
                accessToken = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expires,
                User = new UserDto
                {
                    Id = user.Id,
                    Fullname = user.FullName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    Role = roles.FirstOrDefault() ?? "Customer"
                }
            };
        }
    }
}
