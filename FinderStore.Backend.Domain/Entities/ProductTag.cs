using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.Entities
{
    public class ProductTag
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Slug { get; private set; }

        public Guid ProductId { get; private set; }
        public Product Product { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private ProductTag() { } // For EF Core

        public static ProductTag Create(string name, Guid productId)
        {
            return new ProductTag
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = name.ToLower().Replace(" ", "-"),
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
