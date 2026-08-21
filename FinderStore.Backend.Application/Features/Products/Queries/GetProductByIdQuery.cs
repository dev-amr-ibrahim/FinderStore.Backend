using AutoMapper;
using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Backend.Application.Features.Products.Queries;

public record GetProductByIdQuery : IRequest<ProductDto?>
{
    public Guid Id { get; init; }
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
        var productDto = product == null ? null : _mapper.Map<ProductDto>(product);

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