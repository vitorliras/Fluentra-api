using Fluentra.Domain.Entities.Users;

namespace Fluentra.Domain.Interfaces.Users;

public interface IUserRepository
{
    Task<bool> AddAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
}
