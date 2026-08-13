using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BackupPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }

    public class Address
    {
        public Guid Id { get; set; }
        public string Type { get; set; } // "shipping" or "billing"
        public string FullName { get; set; }
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public bool IsDefault { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

    public class WishlistItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime AddedAt { get; set; }
        public ApplicationUser User { get; set; }
        public Product Product { get; set; }
    }
}
