using AutoMapper;
using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Application.Constants;
using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Backend.Application.Features.Products.Queries;

public record GetProductByIdQuery : IRequest<ProductDto?>
{
    public Guid Id { get; init; }
    public bool IncludeInactive { get; init; }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _productRepository;
    private readonly IBlobStorageService _blobStorageService;

    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper, IBlobStorageService blobStorageService)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _blobStorageService = blobStorageService;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (product != null && !request.IncludeInactive && (!product.IsActive || !product.Category.IsActive))
            return null;
        var productDto = product == null ? null : _mapper.Map<ProductDto>(product);

        if (productDto is not null)
        {
            foreach (var image in productDto.Images)
                image.Url = _blobStorageService.GetPublicUrl(image.Url, BlobContainers.Products);

            if (productDto.Category is not null)
                productDto.Category.ImageUrl = _blobStorageService.GetPublicUrl(productDto.Category.ImageUrl, BlobContainers.Categories);
        }

        #region SAS
        //TODO Used when creating Private Blob To generate SAS
        //var imagesUrls = new List<string>();

        //foreach (var image in productDto.Images)
        //{
        //    string fileName = Path.GetFileName(image.Url);
        //    var imageURL = await _blobStorageService.GenerateReadUrlAsync(fileName, TimeSpan.FromMinutes(15));
        //    image.Url = imageURL;
        //}
        #endregion
        return productDto;
    }
}