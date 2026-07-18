using Fluentra.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fluentra.Test.Api;

// Banco de teste isolado (LocalDB) — nunca o banco de desenvolvimento, conforme
// technology/backend/dotnet/testing.md. Cada instância desta factory usa um nome de
// banco único (não "FluentraTest" fixo), porque cada classe de teste que implementa
// IClassFixture<FluentraWebApplicationFactory> recebe sua própria instância, e o xUnit
// roda classes de teste diferentes em paralelo por padrão — um nome fixo compartilhado
// faz uma classe apagar o banco que a outra ainda está usando.
public sealed class FluentraWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"FluentraTest_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    $@"Server=(localdb)\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True",
                ["Jwt:Issuer"] = "Fluentra.Test",
                ["Jwt:Audience"] = "Fluentra.Test",
                ["Jwt:ExpirationHours"] = "4",
                ["Jwt:Key"] = "test-signing-key-at-least-32-characters-long",
            });
        });

        builder.ConfigureServices(services =>
        {
            var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
        });
    }

    protected override void Dispose(bool disposing)
    {
        // Best-effort: descarta o banco de teste isolado desta instância. Uma falha aqui
        // (ex.: host já finalizado) nunca deve mascarar o resultado real dos testes — só
        // deixaria, na pior hipótese, uma base "FluentraTest_*" órfã no LocalDB local.
        try
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureDeleted();
        }
        catch (ObjectDisposedException)
        {
        }

        base.Dispose(disposing);
    }
}
