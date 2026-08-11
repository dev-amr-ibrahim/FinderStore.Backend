using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; private set; }
        public string Url { get; private set; }
        public string Alt { get; private set; }
        public string? AltAr { get; private set; }
        public bool IsPrimary { get; private set; }
        public int DisplayOrder { get; private set; }

        public Guid ProductId { get; private set; }
        public Product Product { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private ProductImage() { } // For EF Core

        public static ProductImage Create(string url, string alt, string? altAr,
            bool isPrimary, int displayOrder, Guid productId)
        {
            return new ProductImage
            {
                Id = Guid.NewGuid(),
                Url = url,
                Alt = alt,
                AltAr = altAr,
                IsPrimary = isPrimary,
                DisplayOrder = displayOrder,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void SetPrimary(bool isPrimary)
        {
            IsPrimary = isPrimary;
        }

        public void Update(string url, string alt, string? altAr, int displayOrder)
        {
            Url = url;
            Alt = alt;
            AltAr = altAr;
            DisplayOrder = displayOrder;
        }
    }
}