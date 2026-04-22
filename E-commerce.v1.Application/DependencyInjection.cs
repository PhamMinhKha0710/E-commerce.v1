using System.Reflection;
using E_commerce.v1.Application.Behaviors;
using E_commerce.v1.Application.Features.Categories.Services;
using E_commerce.v1.Application.Features.Coupons.Services;
using E_commerce.v1.Application.Features.Products.Services;
using E_commerce.v1.Application.Features.Promotions.Services;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Common.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace E_commerce.v1.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddScoped<IProductSlugService, ProductSlugService>();
        services.AddScoped<ICategorySlugService, CategorySlugService>();
        services.AddScoped<ICouponCalculationService, CouponCalculationService>();
        services.AddScoped<IPromotionRuleBuilderService, PromotionRuleBuilderService>();

        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }
}
