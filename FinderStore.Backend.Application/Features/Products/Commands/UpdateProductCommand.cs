using FinderStore.Backend.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Application.Features.Products.Commands;

public record UpdateProductCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string NameAr { get; init; }
    public string Description { get; init; }
    public string DescriptionAr { get; init; }
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public int StockQuantity { get; init; }
    public Guid CategoryId { get; init; }
    public string UpdatedBy { get; init; }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            throw new KeyNotFoundException($"Product with ID {request.Id} not found");

        product.Update(
            request.Name,
            request.NameAr,
            request.Description,
            request.DescriptionAr,
            request.Price,
            request.CompareAtPrice,
            request.StockQuantity,
            request.CategoryId,
            request.UpdatedBy);

        await _context.SaveChangesAsync(cancellationToken);
    }
}