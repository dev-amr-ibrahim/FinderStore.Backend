using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.Entities
{
    public class ProductVariant
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string NameAr { get; private set; }

        public Guid ProductId { get; private set; }
        public Product Product { get; private set; }

        public ICollection<VariantOption> Options { get; private set; } = new List<VariantOption>();

        public DateTime CreatedAt { get; private set; }

        private ProductVariant() { } // For EF Core

        public static ProductVariant Create(string name, string nameAr, Guid productId)
        {
            return new ProductVariant
            {
                Id = Guid.NewGuid(),
                Name = name,
                NameAr = nameAr,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public class VariantOption
    {
        public Guid Id { get; private set; }
        public string Value { get; private set; }
        public string? ValueAr { get; private set; }
        public decimal? PriceAdjustment { get; private set; }
        public bool IsAvailable { get; private set; }
        public int StockQuantity { get; private set; }

        public Guid ProductVariantId { get; private set; }
        public ProductVariant ProductVariant { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private VariantOption() { } // For EF Core

        public static VariantOption Create(string value, string? valueAr,
            decimal? priceAdjustment, int stockQuantity, Guid productVariantId)
        {
            return new VariantOption
            {
                Id = Guid.NewGuid(),
                Value = value,
                ValueAr = valueAr,
                PriceAdjustment = priceAdjustment,
                IsAvailable = stockQuantity > 0,
                StockQuantity = stockQuantity,
                ProductVariantId = productVariantId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateStock(int quantity)
        {
            StockQuantity = quantity;
            IsAvailable = quantity > 0;
        }
    }
}