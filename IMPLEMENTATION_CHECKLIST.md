# Implementation Checklist & Controller Example

## ✅ Implementation Checklist

- [x] Created `ProductImageService` (Domain Service)
  - Location: `FinderStore.Backend.Domain/Services/ProductImageService.cs`
  - Implements business rules for image management
  - Validates constraints (max 10 images, primary image logic)

- [x] Updated `Product` aggregate root
  - Location: `FinderStore.Backend.Domain/Entities/Product.cs`
  - Added `AddImage()` method
  - Added `RemoveImage()` method

- [x] Created `ProductApplicationService`
  - Location: `FinderStore.Backend.Application/Services/ProductApplicationService.cs`
  - Orchestrates blob upload and domain operations
  - Handles transactional consistency
  - Cleans up blobs on failure

- [x] Updated `CreateProductCommand`
  - Location: `FinderStore.Backend.Application/Features/Products/Commands/CreateProductCommand.cs`
  - Added `ProductImages` collection property
  - Created `CreateProductImageDto` for image metadata
  - Updated handler to use ProductApplicationService
  - Supports multiple images with primary selection

- [x] Updated `DependencyInjection.cs`
  - Registered `IProductImageService` → `ProductImageService`
  - Registered `IProductApplicationService` → `ProductApplicationService`

- [ ] **PENDING**: Update your controller (see example below)
- [ ] **PENDING**: Test the implementation
- [ ] **PENDING**: Add database migration for ProductImage table (if needed)

## Controller Implementation Example

### Update Your Product Controller

```csharp
using FinderStore.Backend.Application.Features.Products.Commands;
using FinderStore.Backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinderStore.Backend.API.Controllers.Website
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new product with multiple images
        /// </summary>
        /// <remarks>
        /// Supports file uploads via multipart/form-data
        /// 
        /// Form fields:
        /// - Name (string, required)
        /// - NameAr (string, required)
        /// - Description (string, required)
        /// - DescriptionAr (string, required)
        /// - Price (decimal, required)
        /// - CompareAtPrice (decimal?, optional)
        /// - Sku (string, required)
        /// - StockQuantity (int, required)
        /// - CategoryId (guid, required)
        /// - ProductImages[0].File (file, optional)
        /// - ProductImages[0].Alt (string, required if file present)
        /// - ProductImages[0].AltAr (string, optional)
        /// - ProductImages[0].IsPrimary (boolean, optional)
        /// - ProductImages[1].File, etc. for additional images
        /// </remarks>
        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateProduct(
            [FromForm] CreateProductCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var productId = await _mediator.Send(command, cancellationToken);

                return CreatedAtAction(nameof(GetProductById), 
                    new { id = productId }, 
                    new { id = productId });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get product by ID (example)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            // TODO: Implement Query
            return NotFound();
        }
    }

    public class CreateProductResponse
    {
        public Guid Id { get; set; }
    }

    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public string Sku { get; set; }
        public int StockQuantity { get; set; }
        public List<ProductImageDto> Images { get; set; }
    }

    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Alt { get; set; }
        public string? AltAr { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
    }
}
```

## Postman/Thunder Client Example

### Create Product with Multiple Images

```http
POST http://localhost:5000/api/products/create
Authorization: Bearer {token}
Content-Type: multipart/form-data; boundary=----FormBoundary

------FormBoundary
Content-Disposition: form-data; name="Name"

Premium Laptop
------FormBoundary
Content-Disposition: form-data; name="NameAr"

كمبيوتر محمول متميز
------FormBoundary
Content-Disposition: form-data; name="Description"

High-performance laptop with RTX graphics
------FormBoundary
Content-Disposition: form-data; name="DescriptionAr"

كمبيوتر محمول عالي الأداء برسوميات RTX
------FormBoundary
Content-Disposition: form-data; name="Price"

1299.99
------FormBoundary
Content-Disposition: form-data; name="CompareAtPrice"

1499.99
------FormBoundary
Content-Disposition: form-data; name="Sku"

LAPTOP-PREM-001
------FormBoundary
Content-Disposition: form-data; name="StockQuantity"

15
------FormBoundary
Content-Disposition: form-data; name="CategoryId"

550e8400-e29b-41d4-a716-446655440000
------FormBoundary
Content-Disposition: form-data; name="CreatedBy"

admin@finder.local
------FormBoundary
Content-Disposition: form-data; name="ProductImages[0].File"; filename="laptop-front.jpg"
Content-Type: image/jpeg

[binary image data]
------FormBoundary
Content-Disposition: form-data; name="ProductImages[0].Alt"

Laptop front view with open screen
------FormBoundary
Content-Disposition: form-data; name="ProductImages[0].AltAr"

منظر أمامي للكمبيوتر مع الشاشة المفتوحة
------FormBoundary
Content-Disposition: form-data; name="ProductImages[0].IsPrimary"

true
------FormBoundary
Content-Disposition: form-data; name="ProductImages[1].File"; filename="laptop-side.jpg"
Content-Type: image/jpeg

[binary image data]
------FormBoundary
Content-Disposition: form-data; name="ProductImages[1].Alt"

Laptop side view showing ports
------FormBoundary
Content-Disposition: form-data; name="ProductImages[1].AltAr"

منظر جانبي للكمبيوتر يظهر المنافذ
------FormBoundary
Content-Disposition: form-data; name="ProductImages[1].IsPrimary"

false
------FormBoundary--
```

### cURL Example

```bash
curl -X POST http://localhost:5000/api/products/create \
  -H "Authorization: Bearer {token}" \
  -F "Name=Premium Laptop" \
  -F "NameAr=كمبيوتر محمول متميز" \
  -F "Description=High-performance laptop" \
  -F "DescriptionAr=كمبيوتر محمول عالي الأداء" \
  -F "Price=1299.99" \
  -F "CompareAtPrice=1499.99" \
  -F "Sku=LAPTOP-PREM-001" \
  -F "StockQuantity=15" \
  -F "CategoryId=550e8400-e29b-41d4-a716-446655440000" \
  -F "CreatedBy=admin@finder.local" \
  -F "ProductImages[0].File=@/path/to/laptop-front.jpg" \
  -F "ProductImages[0].Alt=Laptop front view" \
  -F "ProductImages[0].AltAr=منظر أمامي" \
  -F "ProductImages[0].IsPrimary=true" \
  -F "ProductImages[1].File=@/path/to/laptop-side.jpg" \
  -F "ProductImages[1].Alt=Laptop side view" \
  -F "ProductImages[1].IsPrimary=false"
```

## Database Setup

### EF Core Migration (if ProductImage table not yet created)

```bash
# In Package Manager Console or command line
Add-Migration AddProductImages -Project FinderStore.Backend.Infrastructure

# Or using dotnet CLI
dotnet ef migrations add AddProductImages --project FinderStore.Backend.Infrastructure --startup-project FinderStore.Backend.API

# Then apply
Update-Database -Project FinderStore.Backend.Infrastructure

# Or with dotnet CLI
dotnet ef database update --project FinderStore.Backend.Infrastructure --startup-project FinderStore.Backend.API
```

## Configuration Required

Ensure your `appsettings.json` has Azure Blob Storage configuration:

```json
{
  "AzureStorage": {
    "AccountName": "youraccountname",
    "ContainerName": "products"
  }
}
```

## Testing the Implementation

### Test Scenario 1: Single Image
```csharp
[Test]
public async Task CreateProduct_WithSingleImage_Success()
{
    // Arrange
    var command = new CreateProductCommand
    {
        Name = "Test Product",
        NameAr = "منتج اختبار",
        Description = "Test",
        DescriptionAr = "اختبار",
        Price = 99.99m,
        Sku = "TEST-001",
        StockQuantity = 10,
        CategoryId = Guid.NewGuid(),
        CreatedBy = "test@test.com",
        ProductImages = new List<CreateProductImageDto>
        {
            new CreateProductImageDto
            {
                File = mockFile,
                Alt = "Main image",
                AltAr = "الصورة الرئيسية",
                IsPrimary = true
            }
        }
    };

    // Act
    var handler = new CreateProductCommandHandler(dbContext, appService);
    var productId = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.That(productId, Is.Not.EqualTo(Guid.Empty));
    var product = dbContext.Products.Include(p => p.Images).First();
    Assert.That(product.Images.Count, Is.EqualTo(1));
    Assert.That(product.Images.First().IsPrimary, Is.True);
}
```

### Test Scenario 2: Multiple Images
```csharp
[Test]
public async Task CreateProduct_WithMultipleImages_Success()
{
    var command = new CreateProductCommand
    {
        // ... basic fields ...
        ProductImages = new List<CreateProductImageDto>
        {
            new { File = img1, Alt = "Image 1", IsPrimary = false },
            new { File = img2, Alt = "Image 2", IsPrimary = true },
            new { File = img3, Alt = "Image 3", IsPrimary = false }
        }
    };

    var productId = await handler.Handle(command, CancellationToken.None);

    var product = dbContext.Products.Include(p => p.Images).First();
    Assert.That(product.Images.Count, Is.EqualTo(3));
    Assert.That(product.Images.Count(i => i.IsPrimary), Is.EqualTo(1));
    Assert.That(product.Images.First(i => i.IsPrimary).DisplayOrder, Is.EqualTo(2));
}
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Image service not registered" | Ensure DependencyInjection.cs has service registrations |
| "Maximum images exceeded" | Domain service enforces 10-image limit; reduce image count |
| "Blob upload fails but product created" | Ensure Azure credentials are valid; check account name in appsettings |
| "Alt text required" | Provide Alt property in ProductImages |
| "File not recognized" | Ensure IFormFile is properly serialized in multipart request |
| "Primary image not set" | First image defaults to primary if none marked as primary |
