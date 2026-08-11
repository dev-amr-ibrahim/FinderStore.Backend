using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Application.DTOs
{
    public class PaymentMethodDto
    {
        public string Type { get; set; }
        public string? Last4Digits { get; set; }
        public string? Brand { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string Status { get; set; }
    }
}
