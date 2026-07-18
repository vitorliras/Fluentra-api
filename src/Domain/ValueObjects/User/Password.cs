using System.Text.RegularExpressions;
using Fluentra.Domain.Exceptions;

namespace Fluentra.Domain.ValueObjects.User;

// Regra decidida em Preparação/10-login-cadastro/01-cadastro.md: mínimo 8 caracteres,
// pelo menos 1 número e 1 caractere especial. O valor nunca é persistido diretamente —
// só existe transitoriamente até virar hash (ver IPasswordHasher).
public sealed partial record Password
{
    private const int MinLength = 8;

    public string Value { get; }

    public Password(string value)
    {
        if (string.IsNullOrEmpty(value)
            || value.Length < MinLength
            || !DigitRegex().IsMatch(value)
            || !SpecialCharacterRegex().IsMatch(value))
            throw new DomainException("InvalidPassword");

        Value = value;
    }

    [GeneratedRegex(@"\d")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex SpecialCharacterRegex();
}
