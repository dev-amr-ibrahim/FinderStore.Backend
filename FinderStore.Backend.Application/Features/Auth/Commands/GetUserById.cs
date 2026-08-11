using FinderStore.Backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FinderStore.Backend.Application.Features.Auth.Commands
{
    public record GetUserByIdQuery : IRequest<UserDto>
    {
        public Guid UserId { get; init; }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Fullname = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Role = roles.FirstOrDefault() ?? "Customer"
            };
        }
    }
}
