using System.Reflection;
using E_commerce.v1.Application.Behaviors;
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
        
        return services;
    }
}
