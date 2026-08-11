using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FinderStore.Backend.Application.Features.Products.Commands
{
    public record CreateProductCommand : IRequest<Guid>
    {
        public string Name { get; init; }
        public string NameAr { get; init; }
        public string Description { get; init; }
        public string DescriptionAr { get; init; }
        public decimal Price { get; init; }
        public decimal? CompareAtPrice { get; init; }
        public string Sku { get; init; }
        public int StockQuantity { get; init; }
        public Guid CategoryId { get; init; }
        public string CreatedBy { get; init; }
    }
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CategoryId).NotEmpty();
        }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateProductCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = Product.Create(
                request.Name,
                request.NameAr,
                request.Description,
                request.DescriptionAr,
                request.Price,
                request.CompareAtPrice,
                request.Sku,
                request.StockQuantity,
                request.CategoryId,
                request.CreatedBy);

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
