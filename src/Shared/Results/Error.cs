namespace Fluentra.Shared.Results;

public sealed class Error
{
    public string Code { get; }

    private Error(string code)
    {
        Code = code;
    }

    public static Error From(string code) => new(code);
}
