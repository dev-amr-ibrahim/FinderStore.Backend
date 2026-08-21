using System.Reflection;
using FinderStore.Backend.Application.Common.Behaviours;
using FinderStore.Backend.Application.Services;
using FinderStore.Backend.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FinderStore.Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Correct way - pass Assembly directly
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<Common.Mappings.MappingProfile>();
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        // Register Domain Services
        services.AddScoped<IProductImageService, ProductImageService>();

        // Register Application Services
        services.AddScoped<IProductApplicationService, ProductApplicationService>();

        return services;
    }
}