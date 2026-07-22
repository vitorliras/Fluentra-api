using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Fluentra.Infrastructure.ExternalServices.Shadowing;
using Fluentra.Infrastructure.Persistence;
using Fluentra.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Fluentra.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.Configure<YouTubeSettings>(configuration.GetSection("YouTube"));
        services.AddHttpClient<IVideoSearchProvider, YouTubeVideoSearchProvider>(client =>
        {
            client.BaseAddress = new Uri(configuration["YouTube:BaseUrl"] ?? "https://www.googleapis.com/youtube/v3/");
        });
        services.AddScoped<IYouTubeQuotaTracker, YouTubeQuotaTracker>();
        services.AddScoped<IVideoTranscriptProvider, YoutubeExplodeTranscriptProvider>();

        services.Configure<WhisperSettings>(configuration.GetSection("Whisper"));
        services.AddSingleton<ISpeechTranscriber, WhisperSpeechTranscriber>();

        services.AddHttpClient<ITranslationProvider, LibreTranslateProvider>(client =>
        {
            client.BaseAddress = new Uri(configuration["LibreTranslate:BaseUrl"] ?? "http://127.0.0.1:5001/");
        });

        services.Scan(scan => scan
            .FromAssemblyOf<AppDbContext>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
            .AsMatchingInterface()
            .WithScopedLifetime());

        return services;
    }
}
