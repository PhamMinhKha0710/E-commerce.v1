using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Infrastructure.Data;
using E_commerce.v1.Infrastructure.Repositories;
using E_commerce.v1.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_commerce.v1.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICheckoutRepository, CheckoutRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductQueryRepository, ProductQueryRepository>();
        services.AddScoped<IPromotionRuleRepository, PromotionRuleRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IVariantRepository, VariantRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Auth
        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        return services;
    }
}
