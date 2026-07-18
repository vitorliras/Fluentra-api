namespace Fluentra.Shared.Messages;

public static class AuthErrorCodes
{
    // Mensagem genérica de propósito — nunca revela se o problema foi o identificador
    // (e-mail/username) ou a senha, seguindo o princípio anti-enumeração de usuários
    // já validado no KronPay (ver Preparação/10-login-cadastro/02-login-e-token.md).
    public const string InvalidCredentials = "InvalidCredentials";
}
