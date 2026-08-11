using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Application.DTOs
{
    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Alt { get; set; }
        public string? AltAr { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
    }
}
