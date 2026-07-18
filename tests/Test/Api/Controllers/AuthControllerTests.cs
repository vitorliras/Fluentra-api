using System.Net;
using System.Net.Http.Json;
using Fluentra.Test.Api;
using Shouldly;

namespace Fluentra.Test.Api.Controllers;

public sealed class AuthControllerTests : IClassFixture<FluentraWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(FluentraWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Return_Ok_When_Login_With_Valid_Credentials()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login{suffix}@example.com";
        var username = $"loginuser{suffix}";
        const string password = "Senha123!";

        await _client.PostAsJsonAsync("/users", new
        {
            name = "Usuário de Login",
            username,
            email,
            password,
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new { identifier = email, password });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_Credentials_Are_Invalid()
    {
        var response = await _client.PostAsJsonAsync(
            "/auth/login",
            new { identifier = "naoexiste@example.com", password = "SenhaErrada1!" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
