using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using FinderStore.Backend.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FinderStore.Backend.Application.Features.Auth.Commands
{
    public record RegisterCommand : IRequest<AuthResponse>
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
        public string ConfirmPassword { get; init; }
    }

    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match");
        }
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email already registered");

            // Create new user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = $"{request.FirstName} {request.LastName}" ,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true // Auto-confirm for now, add email verification later
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }

            // Assign Customer role
            await _userManager.AddToRoleAsync(user, UserRoles.Customer);

            // Generate tokens
            var roles = new List<string> { UserRoles.Customer };
            var (token, expires) = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expires,
                User = new UserDto
                {
                    Id = user.Id,
                    Fullname = $"{request.FirstName} {request.LastName}",
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    Role = UserRoles.Customer
                }
            };
        }
    }
}
