using System.IdentityModel.Tokens.Jwt;
using Fluentra.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Fluentra.Infrastructure.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub);
            return claim is not null ? int.Parse(claim.Value) : 0;
        }
    }
}
