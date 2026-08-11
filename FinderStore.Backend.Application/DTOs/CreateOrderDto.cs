using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Application.DTOs
{
    public class CreateOrderDto
    {
        public ShippingAddressDto ShippingAddress { get; set; }
        public BillingAddressDto BillingAddress { get; set; }
        public PaymentMethodDto PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public string? CouponCode { get; set; }
    }
}
