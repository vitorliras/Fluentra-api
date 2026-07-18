using Fluentra.Domain.Entities.Users;
using Fluentra.Domain.ValueObjects.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fluentra.Infrastructure.Mappings.Users;

public sealed class UserMap : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasConversion(name => name.Value, value => new Name(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Username)
            .HasConversion(username => username.Value, value => new Username(value))
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.Username)
            .IsUnique();

        builder.Property(x => x.Email)
            .HasConversion(email => email.Value, value => new Email(value))
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .IsRequired();
    }
}
