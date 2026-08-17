# Product Image Management - DDD/CQRS Implementation Guide

## Architecture Overview

This implementation maintains clean DDD/CQRS boundaries while handling blob storage for product images.

### Layer Breakdown

```
┌─────────────────────────────────────────────────────────────┐
│                  API Controller Layer                        │
│  (Receives IFormFile, creates CreateProductCommand)         │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│              CQRS Command Layer                              │
│  - CreateProductCommand (Orchestrates workflow)             │
│  - CreateProductCommandHandler (Entry point)                │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│          Application Service Layer                           │
│  - ProductApplicationService (Bridges Infrastructure)       │
│  - Orchestrates blob upload + domain operations             │
│  - Handles transactional concerns                           │
└──────────────────────┬──────────────────────────────────────┘
                       │
        ┌──────────────┴──────────────┐
        │                             │
┌───────▼─────────────────┐  ┌────────▼──────────────────┐
│   Domain Service Layer  │  │  Infrastructure Service   │
│  - ProductImageService  │  │   - BlobStorageService    │
│  (Business Rules)       │  │   (Technical Operations)  │
│                         │  │                            │
│ • Max 10 images/product │  │ • Upload to Azure Blob    │
│ • Primary image logic   │  │ • Delete from blob        │
│ • Image validation      │  │ • Generate URLs           │
└───────┬─────────────────┘  └────────┬──────────────────┘
        │                             │
┌───────▼─────────────────────────────▼──────────────────────┐
│              Domain Layer (Aggregate Root)                  │
│  - Product (Aggregate Root)                                 │
│  - ProductImage (Aggregate Child)                           │
│                                                              │
│ Methods:                                                     │
│  • AddImage(ProductImage)                                  │
│  • RemoveImage(ProductImage)                               │
│  • Create() static factory                                 │
└──────────────────────────────────────────────────────────────┘
        │
┌───────▼──────────────────────────────────────────────────────┐
│                Database (EF Core)                            │
│  - Products table (Aggregate Root)                           │
│  - ProductImages table (Child entity)                        │
└───────────────────────────────────────────────────────────────┘
```

## Why This Approach is DDD/CQRS Compliant

### ✅ DDD Compliance

1. **Aggregate Root (Product)**
   - Owns all business logic related to images
   - Enforces invariants through domain service
   - Has clear boundaries

2. **Domain Service (ProductImageService)**
   - Implements business rules that don't belong in the aggregate
   - Rules: max 10 images, primary image logic, validation
   - Not a stateless utility; contains domain logic

3. **Value Objects / Child Entities (ProductImage)**
   - Represents a single product image
   - Immutable except for specific methods (SetPrimary, Update)
   - Can only be created through factory method

4. **Separation of Concerns**
   - Infrastructure (BlobStorage) ≠ Domain
   - Application orchestrates both layers
   - Domain doesn't know about file uploads

### ✅ CQRS Compliance

1. **Command Handler**
   - Only handles commands, doesn't return complex objects
   - Uses application service to coordinate
   - Delegates to domain for validation

2. **Single Responsibility**
   - Command: Defines intent
   - Handler: Orchestrates
   - Services: Execute

3. **Transaction Boundary**
   - Entire operation (upload + save) completes as one unit
   - Blob upload happens before SaveChangesAsync
   - Rollback scenario: blob exists but product may not (acceptable)

## Usage Example

### Controller
```csharp
[HttpPost("create")]
public async Task<IActionResult> CreateProduct(
    [FromForm] CreateProductCommand command,
    CancellationToken cancellationToken)
{
    var productId = await _mediator.Send(command, cancellationToken);
    return Ok(new { id = productId });
}
```

### Client/Postman (Form Data)
```
POST /api/products/create

Form Data:
- Name: "Laptop"
- NameAr: "كمبيوتر محمول"
- Description: "High-performance laptop"
- DescriptionAr: "..."
- Price: 1299.99
- Sku: "LAPTOP-001"
- CategoryId: [guid]
- CreatedBy: "admin"
- ProductImages[0].File: [binary image file]
- ProductImages[0].Alt: "Laptop front view"
- ProductImages[0].AltAr: "منظر أمامي للكمبيوتر"
- ProductImages[0].IsPrimary: true
- ProductImages[1].File: [binary image file 2]
- ProductImages[1].Alt: "Laptop side view"
```

## Flow Diagram

```
1. Client sends CreateProductCommand with IFormFile[]
   ↓
2. CreateProductCommandHandler receives command
   ↓
3. Create Product aggregate (empty, no images yet)
   ↓
4. Add Product to DbContext (not saved)
   ↓
5. For each ProductImage in command:
   ├─→ ProductApplicationService.UploadAndAddProductImageAsync()
   │    ├─→ BlobStorageService.UploadAsync()
   │    │   └─→ Returns: Image URL
   │    │
   │    └─→ ProductImageService.AddImageToProduct()
   │        ├─→ Validate: Max 10 images? ✓
   │        ├─→ Validate: Alt text present? ✓
   │        ├─→ Unset previous primary if needed ✓
   │        └─→ Product.AddImage() [Domain method]
   │
   └─→ If any step fails, delete blob and throw
   ↓
6. DbContext.SaveChangesAsync()
   ├─→ Saves Product
   └─→ Saves all ProductImages
   ↓
7. Return Product.Id
```

## Handling Failures Gracefully

### Blob Upload Fails
```
Application Service catches exception
  ↓
Doesn't call ProductImageService.AddImageToProduct()
  ↓
Product remains unchanged
  ↓
Exception bubbles to handler
  ↓
Handler doesn't call SaveChangesAsync()
  ↓
Product not created ✓
```

### Domain Rule Violation (e.g., >10 images)
```
ProductImageService throws InvalidOperationException
  ↓
Application Service catches it
  ↓
Deletes uploaded blob
  ↓
Re-throws exception
  ↓
Handler doesn't call SaveChangesAsync()
  ↓
Product not created ✓
```

### Database Save Fails
```
BlobStorage already uploaded
  ↓
SaveChangesAsync() throws
  ↓
Exception propagates
  ↓
Blob orphaned (acceptable) or implement cleanup saga
```

## Testing Strategy

### Unit Tests
```csharp
// Test ProductImageService business rules
[Test]
public void AddImageToProduct_MaxImagesExceeded_ThrowsException()
{
    var product = new Product(...);
    var service = new ProductImageService();
    
    // Add 10 images
    for(int i = 0; i < 10; i++)
        service.AddImageToProduct(product, ...);
    
    // 11th should fail
    Assert.Throws<InvalidOperationException>(() =>
        service.AddImageToProduct(product, ...));
}
```

### Integration Tests
```csharp
// Test full flow
[Test]
public async Task CreateProductCommand_WithImages_SavesSuccessfully()
{
    var command = new CreateProductCommand
    {
        Name = "Test Product",
        ProductImages = new List<CreateProductImageDto>
        {
            new { File = mockFile, Alt = "Image 1", IsPrimary = true }
        }
    };
    
    var handler = new CreateProductCommandHandler(
        dbContext,
        applicationService);
    
    var productId = await handler.Handle(command, CancellationToken.None);
    
    Assert.That(dbContext.Products.Find(productId).Images.Count, Is.EqualTo(1));
}
```

## Key Design Decisions

| Decision | Reason |
|----------|--------|
| Domain Service vs Aggregate Method | Business rules that involve multiple entities should be in domain service, not aggregate |
| Application Service | Orchestrates between infrastructure and domain; handles transactional boundaries |
| Factory Method on ProductImage | Ensures consistency, simplifies creation, centralizes validation |
| Blob path includes ProductId | Organizes blobs by product, easier cleanup, prevents collisions |
| Transactional Boundary | Product and images created together; consistency guaranteed |
| No image deletion before db save | Simpler error handling, acceptable orphaned blobs |

## Future Enhancements

1. **Image Processing**
   - Add thumbnail generation in ProductApplicationService
   - Validate image format/size before upload

2. **Soft Deletes**
   - Add `DeletedAt` to ProductImage
   - Archive blobs instead of deleting

3. **Image Ordering**
   - Implement reorder command
   - Update DisplayOrder efficiently

4. **CDN Integration**
   - Return CDN URLs instead of direct blob URLs
   - Cache invalidation strategy

5. **Event Sourcing**
   - Emit `ProductCreated` domain event
   - Emit `ProductImageAdded` events
   - Handle via subscription (indexing, notifications)
