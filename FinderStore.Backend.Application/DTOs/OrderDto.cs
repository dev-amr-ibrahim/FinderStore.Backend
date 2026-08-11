using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Application.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? Notes { get; set; }

        public ShippingAddressDto ShippingAddress { get; set; }
        public BillingAddressDto BillingAddress { get; set; }
        public PaymentMethodDto PaymentMethod { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
