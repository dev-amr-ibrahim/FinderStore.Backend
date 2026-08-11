using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Application.DTOs
{
    public class UserReviewDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
