using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.ValueObjects
{
    public class PaymentMethod
    {
        public string Type { get; private set; } // "credit_card", "paypal", "apple_pay", "google_pay"
        public string? Last4Digits { get; private set; }
        public string? Brand { get; private set; } // "visa", "mastercard", "amex"
        public string? ExpiryMonth { get; private set; }
        public string? ExpiryYear { get; private set; }
        public string? TransactionId { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime? PaidAt { get; private set; }

        private PaymentMethod() { } // For EF Core

        public PaymentMethod(string type, string? last4Digits = null,
            string? brand = null, string? expiryMonth = null,
            string? expiryYear = null)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Last4Digits = last4Digits;
            Brand = brand;
            ExpiryMonth = expiryMonth;
            ExpiryYear = expiryYear;
            Status = PaymentStatus.Pending;
        }

        public void MarkAsPaid(string transactionId)
        {
            TransactionId = transactionId;
            Status = PaymentStatus.Paid;
            PaidAt = DateTime.UtcNow;
        }

        public void MarkAsFailed()
        {
            Status = PaymentStatus.Failed;
        }

        public void MarkAsRefunded()
        {
            Status = PaymentStatus.Refunded;
        }

        public string GetMaskedCardNumber()
        {
            return Last4Digits != null ? $"**** **** **** {Last4Digits}" : Type;
        }
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Failed = 2,
        Refunded = 3,
        Cancelled = 4
    }
}
