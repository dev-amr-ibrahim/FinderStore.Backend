using MediatR;
using Microsoft.AspNetCore.Mvc;
using FinderStore.Backend.Application.Features.Products.Queries;
using FinderStore.Backend.Application.DTOs;

namespace FinderStore.Backend.API.Controllers.Website;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> GetProducts(
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isFeatured,
        [FromQuery] string? searchTerm,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetProductsQuery
        {
            CategoryId = categoryId,
            IsFeatured = isFeatured,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            Page = page,
            PageSize = pageSize
        };

        var products = await _mediator.Send(query);
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var product = await _mediator.Send(query);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// Get featured products
    /// </summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> GetFeaturedProducts()
    {
        var query = new GetProductsQuery { IsFeatured = true, PageSize = 10 };
        var products = await _mediator.Send(query);
        return Ok(products);
    }

    /// <summary>
    /// Get new arrivals
    /// </summary>
    [HttpGet("new-arrivals")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> GetNewArrivals()
    {
        var query = new GetProductsQuery
        {
            SortBy = "newest",
            PageSize = 12
        };
        var products = await _mediator.Send(query);
        return Ok(products);
    }

    /// <summary>
    /// Search products
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> SearchProducts(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetProductsQuery
        {
            SearchTerm = q,
            Page = page,
            PageSize = pageSize
        };
        var products = await _mediator.Send(query);
        return Ok(products);
    }

    /// <summary>
    /// Get product reviews
    /// </summary>
    [HttpGet("{id:guid}/reviews")]
    [ProducesResponseType(typeof(List<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReviewDto>>> GetProductReviews(Guid id)
    {
        var query = new GetProductReviewsQuery { ProductId = id };
        var reviews = await _mediator.Send(query);
        return Ok(reviews);
    }
}