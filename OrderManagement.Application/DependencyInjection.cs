using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace OrderManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Add FluentValidation validators (injected into handlers)
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Add MediatR
        services.AddMediatR(typeof(DependencyInjection).Assembly);

        // Note: ValidationBehavior removed - validation is now done in handlers returning Result<T>

        return services;
    }
}
