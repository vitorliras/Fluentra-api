using Fluentra.Application.Abstractions;
using Fluentra.Application.Executors;
using Fluentra.Application.Pipelines;
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

        // Registro manual — não seguem convenção de nome/interface 1:1 previsível pelo
        // Scrutor (ver technology/backend/dotnet/dependency-injection.md).
        services.AddScoped<UseCaseExecutor>();
        services.AddScoped(typeof(ValidationPipeline<,>));

        return services;
    }
}
