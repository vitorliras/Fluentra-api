using Fluentra.Domain.ValueObjects.User;

namespace Fluentra.Domain.Entities.Users;

public sealed class User
{
    public int Id { get; private set; }
    public Name Name { get; private set; } = null!;
    public Username Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    protected User()
    {
    }

    public User(Name name, Username username, Email email, string passwordHash)
    {
        Name = name;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
    }
}
