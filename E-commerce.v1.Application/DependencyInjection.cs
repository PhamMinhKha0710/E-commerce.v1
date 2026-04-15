using System.Reflection;
using E_commerce.v1.Application.Behaviors;
using E_commerce.v1.Application.Features.Coupons.Services;
using E_commerce.v1.Application.Features.Products.Services;
using E_commerce.v1.Application.Features.Promotions.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace E_commerce.v1.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Tự động tìm và quét FluentValidation quy tắc
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Cài đặt MediatR và nhúng Pipeline Validation tự động vào.
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        
        // Register Domain/Application Services
        services.AddScoped<IProductSlugService, ProductSlugService>();
        services.AddScoped<ICouponCalculationService, CouponCalculationService>();
        services.AddScoped<IPromotionRuleBuilderService, PromotionRuleBuilderService>();
        
        return services;
    }
}
