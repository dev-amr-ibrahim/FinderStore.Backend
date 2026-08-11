using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; private set; }
        public string ProductName { get; private set; }
        public string ProductImageUrl { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public decimal TotalPrice { get; private set; }
        public string? VariantDetails { get; private set; }

        public Guid OrderId { get; private set; }
        public Order Order { get; private set; }

        public Guid ProductId { get; private set; }
        public Product Product { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private OrderItem() { } // For EF Core

        public static OrderItem Create(string productName, string productImageUrl,
            decimal unitPrice, int quantity, string? variantDetails,
            Guid orderId, Guid productId)
        {
            return new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductName = productName,
                ProductImageUrl = productImageUrl,
                UnitPrice = unitPrice,
                Quantity = quantity,
                TotalPrice = unitPrice * quantity,
                VariantDetails = variantDetails,
                OrderId = orderId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
