using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using FinderStore.Backend.Domain.Services;
using Microsoft.AspNetCore.Http;

namespace FinderStore.Backend.Application.Services
{
    /// <summary>
    /// Application Service that orchestrates product operations
    /// Coordinates between Command Handlers, Domain Services, and Infrastructure Services
    /// Follows CQRS by being used only by Command Handlers
    /// </summary>
    public interface IProductApplicationService
    {
        /// <summary>
        /// Uploads a product image and adds it to the product
        /// Handles infrastructure concerns (blob upload) while delegating business logic to domain service
        /// </summary>
        Task UploadAndAddProductImageAsync(
            Product product,
            IFormFile imageFile,
            string altText,
            string? altTextAr,
            bool isPrimary,
            CancellationToken cancellationToken);

        /// <summary>
        /// Removes a product image from blob storage and product
        /// </summary>
        Task RemoveProductImageAsync(
            Product product,
            Guid imageId,
            CancellationToken cancellationToken);
    }

    public class ProductApplicationService : IProductApplicationService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IProductImageService _productImageService;

        public ProductApplicationService(
            IBlobStorageService blobStorageService,
            IProductImageService productImageService)
        {
            _blobStorageService = blobStorageService;
            _productImageService = productImageService;
        }

        public async Task UploadAndAddProductImageAsync(
            Product product,
            IFormFile imageFile,
            string altText,
            string? altTextAr,
            bool isPrimary,
            CancellationToken cancellationToken)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (imageFile == null)
                throw new ArgumentNullException(nameof(imageFile));

            if (string.IsNullOrWhiteSpace(altText))
                throw new ArgumentException("Alt text is required.", nameof(altText));

            // Step 1: Upload image to blob storage (Infrastructure concern)
            await using var stream = imageFile.OpenReadStream();
            var fileName = $"products/{product.Id}/{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

            var imageUrl = await _blobStorageService.UploadAsync(
                stream,
                fileName,
                imageFile.ContentType,
                cancellationToken);

            // Step 2: Add image to product using domain service (Domain concern)
            try
            {
                _productImageService.AddImageToProduct(
                    product,
                    imageUrl,
                    altText,
                    altTextAr,
                    isPrimary);
            }
            catch
            {
                // If domain service fails, clean up the uploaded blob
                await _blobStorageService.DeleteAsync(fileName, cancellationToken);
                throw;
            }
        }

        public async Task RemoveProductImageAsync(
            Product product,
            Guid imageId,
            CancellationToken cancellationToken)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var image = product.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
                throw new InvalidOperationException("Image not found.");

            var imageUrl = image.Url;

            // Remove from product using domain service
            _productImageService.RemoveImageFromProduct(product, imageId);

            // Delete from blob storage
            try
            {
                // Extract filename from URL for deletion
                var uri = new Uri(imageUrl);
                var fileName = uri.AbsolutePath.TrimStart('/');
                
                await _blobStorageService.DeleteAsync(fileName, cancellationToken);
            }
            catch
            {
                // Log warning: blob might not exist, but domain operation succeeded
                // In a real scenario, consider logging here
            }
        }
    }
}
