namespace Fluentra.Application.DTOs.Users.User;

public sealed record CreateUserRequest(string Name, string Username, string Email, string Password);
