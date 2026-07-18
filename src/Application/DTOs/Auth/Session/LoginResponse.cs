namespace Fluentra.Application.DTOs.Auth.Session;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAt, string Name, string Username);
