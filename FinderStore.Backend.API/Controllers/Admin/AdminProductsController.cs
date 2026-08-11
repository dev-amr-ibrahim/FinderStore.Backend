using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Application.Features.Orders.Queries;
using FinderStore.Backend.Application.Features.Products.Commands;
using FinderStore.Backend.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxeCommerce.API.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin,Manager")]
[Produces("application/json")]
public class AdminProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateProduct([FromBody] CreateProductCommand command)
    {
        command = command with { CreatedBy = User.Identity.Name };
        var productId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProduct), new { id = productId }, productId);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
    {
        command = command with { Id = id, UpdatedBy = User.Identity.Name };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _mediator.Send(new DeleteProductCommand { Id = id });
        return NoContent();
    }

    /// <summary>
    /// Toggle product active status
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ToggleProductActive(Guid id)
    {
        await _mediator.Send(new ToggleProductActiveCommand
        {
            Id = id,
            UpdatedBy = User.Identity.Name
        });
        return NoContent();
    }

    /// <summary>
    /// Toggle product featured status
    /// </summary>
    [HttpPatch("{id:guid}/toggle-featured")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ToggleProductFeatured(Guid id)
    {
        await _mediator.Send(new ToggleProductFeaturedCommand
        {
            Id = id,
            UpdatedBy = User.Identity.Name
        });
        return NoContent();
    }

    /// <summary>
    /// Upload product images
    /// </summary>
    [HttpPost("{id:guid}/images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadProductImages(Guid id, [FromForm] List<IFormFile> files)
    {
        await _mediator.Send(new UploadProductImagesCommand
        {
            ProductId = id,
            Files = files
        });
        return Ok();
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStatsDto>> GetStatistics()
    {
        var stats = await _mediator.Send(new GetDashboardStatsQuery());
        return Ok(stats);
    }

    private async Task<ActionResult> GetProduct(Guid id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery { Id = id });
        return Ok(product);
    }
}