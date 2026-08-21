using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Application.Services;
using FinderStore.Backend.Domain.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using FinderStore.Backend.Domain.Repositories;
using FinderStore.Backend.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

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
        /// <summary>
        /// Multiple product images with metadata
        /// </summary>
        public List<CreateProductImageDto> ProductImages { get; init; } = new();
    }

    /// <summary>
    /// DTO for product image creation
    /// </summary>
    public class CreateProductImageDto
    {
        public IFormFile File { get; set; }
        public string Alt { get; set; }
        public string? AltAr { get; set; }
        public bool IsPrimary { get; set; }
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
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductApplicationService _productApplicationService;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            IProductImageRepository productImageRepository,
            IUnitOfWork unitOfWork,
            IProductApplicationService productApplicationService)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
            _unitOfWork = unitOfWork;
            _productApplicationService = productApplicationService;
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

            await _productRepository.AddAsync(product, cancellationToken);

            if (request.ProductImages?.Any() == true)
            {
                var hasPrimary = request.ProductImages.Any(x => x.IsPrimary);
                
                foreach (var imageDto in request.ProductImages)
                {
                    var isPrimary = imageDto.IsPrimary || (!hasPrimary && request.ProductImages.IndexOf(imageDto) == 0);

                    await _productApplicationService.UploadAndAddProductImageAsync(
                        product,
                        imageDto.File,
                        imageDto.Alt,
                        imageDto.AltAr,
                        isPrimary,
                        cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
