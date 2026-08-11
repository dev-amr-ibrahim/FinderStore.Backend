using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Application.DTOs
{
    public class VariantOptionDto
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public string? ValueAr { get; set; }
        public decimal? PriceAdjustment { get; set; }
        public bool IsAvailable { get; set; }
        public int StockQuantity { get; set; }
    }
}
