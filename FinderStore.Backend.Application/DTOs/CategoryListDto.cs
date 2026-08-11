using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Application.DTOs
{
    public class CategoryListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public int ProductCount { get; set; }
    }
}
