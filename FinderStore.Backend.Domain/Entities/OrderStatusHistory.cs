using FinderStore.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.Entities
{
    public class OrderStatusHistory
    {
        public Guid Id { get; set; }
        public OrderStatus Status { get; set; }
        public string Description { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }

        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}
