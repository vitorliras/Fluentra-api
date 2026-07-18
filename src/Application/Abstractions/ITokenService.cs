using Fluentra.Domain.Entities.Users;

namespace Fluentra.Application.Abstractions;

public sealed record TokenResult(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    TokenResult GenerateToken(User user);
}
