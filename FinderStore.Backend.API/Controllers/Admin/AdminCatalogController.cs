using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Application.Features.Categories.Commands;
using FinderStore.Backend.Application.Features.Categories.Queries;
using FinderStore.Backend.Application.Features.Products.Commands;
using FinderStore.Backend.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinderStore.Backend.API.Controllers.Admin;

[ApiController]
[Route("api/admin/catalog")]
[Authorize(Roles = "Admin,Manager")]
[Produces("application/json")]
public sealed class AdminCatalogController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCatalogController(IMediator mediator) => _mediator = mediator;

    [HttpGet("products")]
    public async Task<ActionResult<List<ProductDto>>> GetProducts([FromQuery] Guid? categoryId, [FromQuery] bool? isActive, [FromQuery] string? searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        => Ok(await _mediator.Send(new GetProductsQuery { CategoryId = categoryId, IsActive = isActive, SearchTerm = searchTerm, IncludeInactive = true, Page = Math.Max(page, 1), PageSize = Math.Clamp(pageSize, 1, 100) }));

    [HttpGet("products/{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery { Id = id, IncludeInactive = true });
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPatch("products/{id:guid}/active")]
    public async Task<IActionResult> SetProductActive(Guid id, [FromBody] SetActiveRequest request)
    {
        var updated = await _mediator.Send(new SetProductActiveCommand(id, request.IsActive, User.Identity?.Name ?? "system"));
        return updated ? NoContent() : NotFound();
    }

    [HttpPut("products/{id:guid}/category")]
    public async Task<IActionResult> AssignProductCategory(Guid id, [FromBody] AssignCategoryRequest request)
    {
        var result = await _mediator.Send(new AssignProductCategoryCommand(id, request.CategoryId, User.Identity?.Name ?? "system"));
        return result switch
        {
            AssignProductCategoryResult.ProductNotFound => NotFound(),
            AssignProductCategoryResult.CategoryNotFound => NotFound("Category not found."),
            _ => NoContent()
        };
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
        => Ok(await _mediator.Send(new GetCategoriesQuery { IncludeSubCategories = true }));

    [HttpGet("categoriesLite")]
    public async Task<ActionResult<List<CategoryLiteDto>>> GetCategoriesLite()
    => Ok(await _mediator.Send(new GetCategoriesLiteQuery { OnlyActive = true }));

    [HttpGet("categories/{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
    {
        var category = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<Guid>> CreateCategory([FromForm] CategoryUpsertRequest request)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(ToCategoryData(request)));
        return result.Result switch
        {
            CategoryCommandResult.SlugAlreadyExists => Conflict("A category with this slug already exists."),
            _ => CreatedAtAction(nameof(GetCategory), new { id = result.CategoryId }, result.CategoryId)
        };
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpsertRequest request)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand(id, ToCategoryData(request)));
        return result switch
        {
            CategoryCommandResult.CategoryNotFound => NotFound(),
            CategoryCommandResult.SlugAlreadyExists => Conflict("A category with this slug already exists."),
            _ => NoContent()
        };
    }

    [HttpPatch("categories/{id:guid}/active")]
    public async Task<IActionResult> SetCategoryActive(Guid id, [FromBody] SetActiveRequest request)
    {
        var updated = await _mediator.Send(new SetCategoryActiveCommand(id, request.IsActive));
        return updated ? NoContent() : NotFound();
    }

    private static CategoryCommandData ToCategoryData(CategoryUpsertRequest request)
        => new(request.Name, request.NameAr, request.Slug, request.Description, request.DescriptionAr, request.ImageUrl, request.DisplayOrder, request.ParentCategoryId, request.ImageFile);
}

public sealed record SetActiveRequest(bool IsActive);
public sealed record AssignCategoryRequest(Guid CategoryId);
public sealed record CategoryUpsertRequest(string Name, string NameAr, string Slug, string Description, string DescriptionAr, string? ImageUrl, int DisplayOrder, Guid? ParentCategoryId, IFormFile? ImageFile = null);
