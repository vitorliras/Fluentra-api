using Fluentra.Domain.Entities.Users;
using Fluentra.Domain.Interfaces.Users;
using Fluentra.Domain.ValueObjects.User;
using Fluentra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fluentra.Infrastructure.Repositories.Users;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(User user)
    {
        var result = await _context.Set<User>().AddAsync(user);
        return result.State == EntityState.Added;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var value = new Email(email);

        return await _context.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == value);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var value = new Username(username);

        return await _context.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == value);
    }
}
