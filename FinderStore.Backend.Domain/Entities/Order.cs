using FinderStore.Backend.Domain.Enums;
using FinderStore.Backend.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public string OrderNumber { get; private set; }
        public OrderStatus Status { get; private set; }

        public Guid UserId { get; private set; }
        public ApplicationUser User { get; private set; }

        public decimal Subtotal { get; private set; }
        public decimal Tax { get; private set; }
        public decimal Shipping { get; private set; }
        public decimal Discount { get; private set; }
        public decimal Total { get; private set; }

        public string? TrackingNumber { get; private set; }
        public string? Carrier { get; private set; }
        public DateTime? EstimatedDeliveryDate { get; private set; }
        public string? Notes { get; private set; }
        public string? CancellationReason { get; private set; }
        public ShippingAddress ShippingAddress { get; private set; }
        public BillingAddress BillingAddress { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }

        public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; private set; } = new List<OrderStatusHistory>();
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private Order() { }

        public static Order Create(
            Guid userId,
            decimal subtotal,
            decimal tax,
            decimal shipping,
            decimal discount,
            ShippingAddress shippingAddress,
            BillingAddress billingAddress,
            PaymentMethod paymentMethod,
            string? notes = null)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                Status = OrderStatus.Pending,
                UserId = userId,
                Subtotal = subtotal,
                Tax = tax,
                Shipping = shipping,
                Discount = discount,
                Total = subtotal + tax + shipping - discount,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                PaymentMethod = paymentMethod,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            order.AddStatusHistory(OrderStatus.Pending, "Order placed");

            return order;
        }

        public void UpdateStatus(OrderStatus newStatus, string? changedBy = null)
        {
            var oldStatus = Status;
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;

            AddStatusHistory(newStatus, $"Status changed from {oldStatus} to {newStatus}", changedBy);
        }

        public void AddTracking(string trackingNumber, string carrier, DateTime estimatedDelivery)
        {
            TrackingNumber = trackingNumber;
            Carrier = carrier;
            EstimatedDeliveryDate = estimatedDelivery;
            UpdatedAt = DateTime.UtcNow;
            UpdateStatus(OrderStatus.Shipped);
        }

        public void Cancel(string reason, string? cancelledBy = null)
        {
            if (Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel a delivered order");

            CancellationReason = reason;
            UpdateStatus(OrderStatus.Cancelled, cancelledBy);
        }
        public void AddOrderItem(OrderItem item)
        {
            OrderItems.Add(item);
        }
        public void AddNote(string note)
        {
            Notes = string.IsNullOrEmpty(Notes) ? note : $"{Notes}\n{note}";
        }

        private void AddStatusHistory(OrderStatus status, string description, string? changedBy = null)
        {
            StatusHistory.Add(new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                Status = status,
                Description = description,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow,
                OrderId = Id
            });
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }
    }
}
