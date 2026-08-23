using AutoMapper;
using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Domain.Entities;

namespace FinderStore.Backend.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Tags,
                opt => opt.MapFrom(src => src.Tags.Select(t => t.Name).ToList()));

        CreateMap<ProductImage, ProductImageDto>();

        CreateMap<ProductVariant, ProductVariantDto>();

        CreateMap<VariantOption, VariantOptionDto>();

        CreateMap<ProductReview, ReviewDto>()
            .ForMember(dest => dest.User,
                opt => opt.MapFrom(src => new UserReviewDto
                {
                    Id = src.User.Id,
                    FullName = $"{src.User.FullName}",
                    AvatarUrl = src.User.AvatarUrl
                }));

        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ProductCount,
                opt => opt.MapFrom(src => src.Products.Count))
            .ForMember(dest => dest.ParentCategoryName,
                opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null));

        CreateMap<Category, CategoryLiteDto>()
            .ReverseMap();

        CreateMap<Category, CategoryListDto>()
            .ForMember(dest => dest.ProductCount,
                opt => opt.MapFrom(src => src.Products.Count));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<OrderItem, OrderItemDto>();

        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>();

        CreateMap<Order, OrderSummaryDto>()
            .ForMember(dest => dest.ItemCount,
                opt => opt.MapFrom(src => src.OrderItems.Count))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));
    }
}