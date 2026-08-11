using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.Entities
{
    public class ProductReview
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string Comment { get; private set; }
        public int Rating { get; private set; } // 1-5
        public bool IsVerifiedPurchase { get; private set; }
        public int HelpfulCount { get; private set; }
        public bool IsApproved { get; private set; }

        public Guid ProductId { get; private set; }
        public Product Product { get; private set; }

        public Guid UserId { get; private set; }
        public ApplicationUser User { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private ProductReview() { } // For EF Core

        public static ProductReview Create(string title, string comment, int rating,
            bool isVerifiedPurchase, Guid productId, Guid userId)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            return new ProductReview
            {
                Id = Guid.NewGuid(),
                Title = title,
                Comment = comment,
                Rating = rating,
                IsVerifiedPurchase = isVerifiedPurchase,
                IsApproved = true, // Auto-approve for now
                ProductId = productId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Update(string title, string comment, int rating)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            Title = title;
            Comment = comment;
            Rating = rating;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkHelpful()
        {
            HelpfulCount++;
        }

        public void ToggleApproval()
        {
            IsApproved = !IsApproved;
        }
    }
}
