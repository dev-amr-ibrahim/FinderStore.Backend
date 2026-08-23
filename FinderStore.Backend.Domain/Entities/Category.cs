using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string NameAr { get; private set; }
        public string Slug { get; private set; }
        public string Description { get; private set; }
        public string DescriptionAr { get; private set; }
        public string? ImageUrl { get; private set; }
        public bool IsActive { get; private set; }
        public int DisplayOrder { get; private set; }

        public Guid? ParentCategoryId { get; private set; }
        public Category? ParentCategory { get; private set; }
        public ICollection<Category> SubCategories { get; private set; } = new List<Category>();
        public ICollection<Product> Products { get; private set; } = new List<Product>();

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private Category() { }

        public static Category Create(string name, string nameAr, string slug,
            string description, string descriptionAr, string? imageUrl,
            int displayOrder, Guid? parentCategoryId)
        {
            return new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                NameAr = nameAr,
                Slug = slug,
                Description = description,
                DescriptionAr = descriptionAr,
                ImageUrl = imageUrl,
                IsActive = true,
                DisplayOrder = displayOrder,
                ParentCategoryId = parentCategoryId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string nameAr, string slug,
            string description, string descriptionAr, string? imageUrl,
            int displayOrder, Guid? parentCategoryId)
        {
            Name = name;
            NameAr = nameAr;
            Slug = slug;
            Description = description;
            DescriptionAr = descriptionAr;
            ImageUrl = imageUrl;
            DisplayOrder = displayOrder;
            ParentCategoryId = parentCategoryId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ToggleActive()
        {
            IsActive = !IsActive;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
