using System.Net;
using System.Net.Http.Json;
using Fluentra.Test.Api;
using Shouldly;

namespace Fluentra.Test.Api.Controllers;

public sealed class UsersControllerTests : IClassFixture<FluentraWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(FluentraWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Return_Ok_When_Creating_Valid_User()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var payload = new
        {
            name = "Vitor Lira",
            username = $"vitorlira{suffix}",
            email = $"vitor{suffix}@example.com",
            password = "Senha123!",
        };

        var response = await _client.PostAsJsonAsync("/users", payload);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_Email_Already_Exists()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"duplicado{suffix}@example.com";

        var firstPayload = new
        {
            name = "Primeiro Usuário",
            username = $"primeiro{suffix}",
            email,
            password = "Senha123!",
        };
        await _client.PostAsJsonAsync("/users", firstPayload);

        var duplicatePayload = new
        {
            name = "Segundo Usuário",
            username = $"segundo{suffix}",
            email,
            password = "Senha123!",
        };

        var response = await _client.PostAsJsonAsync("/users", duplicatePayload);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
