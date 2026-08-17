using FinderStore.Backend.Domain.Entities;

namespace FinderStore.Backend.Domain.Services
{
    /// <summary>
    /// Domain Service for managing product images
    /// Encapsulates business rules around image management
    /// </summary>
    public interface IProductImageService
    {
        /// <summary>
        /// Adds an image to a product following domain rules
        /// </summary>
        void AddImageToProduct(Product product, string imageUrl, string alt, 
            string? altAr, bool isPrimary = false);

        /// <summary>
        /// Sets the primary image for a product
        /// Ensures only one image is primary
        /// </summary>
        void SetPrimaryImage(Product product, Guid imageId);

        /// <summary>
        /// Removes an image from a product
        /// </summary>
        void RemoveImageFromProduct(Product product, Guid imageId);
    }

    public class ProductImageService : IProductImageService
    {
        private const int MaxImagesPerProduct = 10;

        public void AddImageToProduct(Product product, string imageUrl, string alt,
            string? altAr, bool isPrimary = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Image URL is required.", nameof(imageUrl));

            if (string.IsNullOrWhiteSpace(alt))
                throw new ArgumentException("Alt text is required.", nameof(alt));

            // Business rule: Maximum images per product
            if (product.Images.Count >= MaxImagesPerProduct)
                throw new InvalidOperationException(
                    $"Cannot add more than {MaxImagesPerProduct} images to a product.");

            // If this is the first image or marked as primary, make it primary
            if (product.Images.Count == 0 || isPrimary)
            {
                // Unset previous primary
                foreach (var image in product.Images.Where(i => i.IsPrimary))
                {
                    image.SetPrimary(false);
                }
                isPrimary = true;
            }

            var displayOrder = product.Images.Max(i => (int?)i.DisplayOrder) ?? 0;

            var productImage = ProductImage.Create(
                imageUrl,
                alt,
                altAr,
                isPrimary,
                displayOrder + 1,
                product.Id);

            product.AddImage(productImage);
        }

        public void SetPrimaryImage(Product product, Guid imageId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var image = product.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
                throw new InvalidOperationException("Image not found in product.");

            // Unset all other images as primary
            foreach (var img in product.Images.Where(i => i.IsPrimary))
            {
                img.SetPrimary(false);
            }

            image.SetPrimary(true);
        }

        public void RemoveImageFromProduct(Product product, Guid imageId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var image = product.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
                throw new InvalidOperationException("Image not found in product.");

            if (product.Images.Count == 1)
                throw new InvalidOperationException("Cannot remove the last image from a product.");

            product.RemoveImage(image);
        }
    }
}
