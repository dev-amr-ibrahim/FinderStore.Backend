using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FinderStore.Backend.Application.Features.Auth.Commands
{
    public record GetUserProfileByIdQuery : IRequest<ProfileDto>
    {
        public Guid UserId { get; init; }
    }

    public class GetUserProfileByIdQueryHandler : IRequestHandler<GetUserProfileByIdQuery, ProfileDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;

        public GetUserProfileByIdQueryHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<ProfileDto> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            var userAddresses = _context.Addresses.Where(a => a.UserId == user.Id).ToList();

            return new ProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                BackupPhone = user.BackupPhone,
                Addresses = userAddresses.Select(address => new ProfileAddressDto
                {
                    Label = address.Type,
                    Recipient = address.FullName,
                    Line1 = address.AddressLine1,
                    Line2 = address.AddressLine2,
                    City = address.City,
                    Region = string.IsNullOrEmpty(address.State) ? null : address.State,
                    PostalCode = string.IsNullOrEmpty(address.ZipCode) ? null : address.ZipCode,
                    Country = address.Country
                }).ToList()
            };
        }
    }
}
