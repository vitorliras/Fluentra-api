using Fluentra.Application.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Fluentra.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<IUseCase<object, object>>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("UseCase")))
            .AsSelf()
            .WithScopedLifetime());

        services.AddValidatorsFromAssemblyContaining<IUseCase<object, object>>();

        return services;
    }
}
