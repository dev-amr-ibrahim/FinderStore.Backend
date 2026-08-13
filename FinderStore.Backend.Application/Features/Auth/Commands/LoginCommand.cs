using FinderStore.Backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using FinderStore.Backend.Application.Common.Interfaces;

namespace FinderStore.Backend.Application.Features.Auth.Commands
{
    public record LoginCommand : IRequest<AuthResponse>
    {
        public string Email { get; init; }
        public string Password { get; init; }
    }
    public record AuthResponse
    {
        public string accessToken { get; init; }
        public string RefreshToken { get; init; }
        public DateTime ExpiresAt { get; init; }
        public UserDto User { get; init; }
    }
    public record UserDto
    {
        public Guid Id { get; init; }
        public string Fullname { get; init; }
        public string Email { get; init; }
        public string Phone { get; init; }
        public string AvatarUrl { get; init; }
        public string Role { get; init; }
    }
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _tokenService = tokenService;
        }
        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated");


            var roles = await _userManager.GetRolesAsync(user);
            var (token, expires) = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                accessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(2),
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