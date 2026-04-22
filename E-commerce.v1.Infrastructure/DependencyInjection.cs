using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Common.Payments;
using E_commerce.v1.Application.Common.Shipping;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.Repositories;
using E_commerce.v1.Infrastructure.Security;
using E_commerce.v1.Infrastructure.ExternalServices.Shipping;
using E_commerce.v1.Infrastructure.ExternalServices.Payments;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_commerce.v1.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SlowQueryLoggingInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) =>
            options
                .UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .AddInterceptors(sp.GetRequiredService<SlowQueryLoggingInterceptor>()));
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartReadRepository, CartReadRepository>();
        services.AddScoped<ICheckoutRepository, CheckoutRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICouponReadRepository, CouponReadRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReviewReadRepository, ReviewReadRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductQueryRepository, ProductQueryRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<IPromotionRuleRepository, PromotionRuleRepository>();
        services.AddMemoryCache();
        services.AddScoped<PromotionRuleReadRepository>();
        services.AddScoped<IPromotionRuleReadRepository, CachedPromotionRuleReadRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IVariantRepository, VariantRepository>();
        services.AddScoped<IVariantReadRepository, VariantReadRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.Configure<AhamoveOptions>(configuration.GetSection(AhamoveOptions.SectionName));
        services.AddHttpClient<IAhamoveClient, AhamoveClient>();

        services.Configure<PayosOptions>(configuration.GetSection(PayosOptions.SectionName));
        services.AddSingleton<IPayosClient, PayosClient>();
        services.AddSingleton<IPayosWebhookVerifier, PayosWebhookVerifier>();

        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        return services;
    }
}
