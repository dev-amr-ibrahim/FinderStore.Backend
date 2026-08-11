using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string NameAr { get; private set; }
        public string Description { get; private set; }
        public string DescriptionAr { get; private set; }
        public decimal Price { get; private set; }
        public decimal? CompareAtPrice { get; private set; }
        public string Sku { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsFeatured { get; private set; }
        public int StockQuantity { get; private set; }
        public double Rating { get; private set; }
        public int ReviewCount { get; private set; }

        // Navigation Properties
        public Guid CategoryId { get; private set; }
        public Category Category { get; private set; }
        public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();
        public ICollection<ProductVariant> Variants { get; private set; } = new List<ProductVariant>();
        public ICollection<ProductReview> Reviews { get; private set; } = new List<ProductReview>();
        public ICollection<ProductTag> Tags { get; private set; } = new List<ProductTag>();

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string? CreatedBy { get; private set; }
        public string? UpdatedBy { get; private set; }

        private Product() { } // For EF Core

        public static Product Create(
            string name,
            string nameAr,
            string description,
            string descriptionAr,
            decimal price,
            decimal? compareAtPrice,
            string sku,
            int stockQuantity,
            Guid categoryId,
            string createdBy)
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                NameAr = nameAr,
                Description = description,
                DescriptionAr = descriptionAr,
                Price = price,
                CompareAtPrice = compareAtPrice,
                Sku = sku,
                IsActive = true,
                StockQuantity = stockQuantity,
                CategoryId = categoryId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void Update(string name, string nameAr, string description,
            string descriptionAr, decimal price, decimal? compareAtPrice,
            int stockQuantity, Guid categoryId, string updatedBy)
        {
            Name = name;
            NameAr = nameAr;
            Description = description;
            DescriptionAr = descriptionAr;
            Price = price;
            CompareAtPrice = compareAtPrice;
            StockQuantity = stockQuantity;
            CategoryId = categoryId;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void ToggleActive(string updatedBy)
        {
            IsActive = !IsActive;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void ToggleFeatured(string updatedBy)
        {
            IsFeatured = !IsFeatured;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
    }
}
