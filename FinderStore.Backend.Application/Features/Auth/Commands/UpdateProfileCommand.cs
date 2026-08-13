using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Backend.Application.Features.Auth.Commands;

public record UpdateProfileCommand : IRequest<ProfileDto>
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? BackupPhone { get; init; }
    public IReadOnlyCollection<ProfileAddressRequest> Addresses { get; init; } = [];
}

public record ProfileAddressRequest
{
    public string Label { get; init; } = string.Empty;
    public string Recipient { get; init; } = string.Empty;
    public string Line1 { get; init; } = string.Empty;
    public string? Line2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string? Region { get; init; }
    public string? PostalCode { get; init; }
    public string Country { get; init; } = string.Empty;
}

public record ProfileDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? BackupPhone { get; init; }
    public string AvatarUrl { get; set; } = string.Empty;
    public string Role = string.Empty;
    public IReadOnlyCollection<ProfileAddressDto> Addresses { get; init; } = [];
}

public record ProfileAddressDto
{
    public string Label { get; init; } = string.Empty;
    public string Recipient { get; init; } = string.Empty;
    public string Line1 { get; init; } = string.Empty;
    public string? Line2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string? Region { get; init; }
    public string? PostalCode { get; init; }
    public string Country { get; init; } = string.Empty;
}

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BackupPhone).MaximumLength(50);
        RuleForEach(x => x.Addresses).SetValidator(new ProfileAddressRequestValidator());
    }
}

public class ProfileAddressRequestValidator : AbstractValidator<ProfileAddressRequest>
{
    public ProfileAddressRequestValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Recipient).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Line2).MaximumLength(500);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Region).MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(30);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ProfileDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;

    public UpdateProfileCommandHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<ProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.Include(x => x.Addresses)
            .SingleOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user is null)
            throw new KeyNotFoundException("User not found");

        var normalizedEmail = _userManager.NormalizeEmail(request.Email);
        var existingUser = await _context.Users.AsNoTracking()
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
        if (existingUser is not null && existingUser.Id != user.Id)
            throw new InvalidOperationException("Email already registered");

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.UserName = user.Email;
        user.PhoneNumber = request.Phone.Trim();
        user.BackupPhone = string.IsNullOrWhiteSpace(request.BackupPhone) ? null : request.BackupPhone.Trim();

        _context.Addresses.RemoveRange(user.Addresses);
        var addresses = request.Addresses.Select((address, index) => new Address
        {
            Id = Guid.NewGuid(), Type = address.Label.Trim(), FullName = address.Recipient.Trim(),
            AddressLine1 = address.Line1.Trim(),
            AddressLine2 = string.IsNullOrWhiteSpace(address.Line2) ? null : address.Line2.Trim(),
            City = address.City.Trim(), State = address.Region?.Trim() ?? string.Empty,
            ZipCode = address.PostalCode?.Trim() ?? string.Empty, Country = address.Country.Trim(),
            Phone = user.PhoneNumber, IsDefault = index == 0, UserId = user.Id
        }).ToList();
        await _context.Addresses.AddRangeAsync(addresses, cancellationToken);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Description)));

        await _context.SaveChangesAsync(cancellationToken);

        return new ProfileDto
        {
            Id = user.Id, FullName = user.FullName, Email = user.Email, Phone = user.PhoneNumber,
            BackupPhone = user.BackupPhone,
            Addresses = addresses.Select(address => new ProfileAddressDto
            {
                Label = address.Type, Recipient = address.FullName, Line1 = address.AddressLine1,
                Line2 = address.AddressLine2, City = address.City,
                Region = string.IsNullOrEmpty(address.State) ? null : address.State,
                PostalCode = string.IsNullOrEmpty(address.ZipCode) ? null : address.ZipCode,
                Country = address.Country
            }).ToList()
        };
    }
}


