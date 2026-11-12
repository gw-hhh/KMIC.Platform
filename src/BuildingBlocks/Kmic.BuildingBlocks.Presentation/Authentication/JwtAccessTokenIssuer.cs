using System.Globalization;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>
/// 用 RS256 签发访问令牌，和验证用同一套签名密钥与签发者、受众配置
/// </summary>
internal sealed class JwtAccessTokenIssuer(SigningKeyStore keys, IOptions<KmicAuthOptions> options) : IAccessTokenIssuer
{
    private readonly JsonWebTokenHandler _handler = new();

    public AccessToken Issue(UserId user, SessionId session, RoleId role, string clientType, DateTimeOffset now, TimeSpan? lifetime = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientType);

        var auth = options.Value;
        var expires = now + (lifetime ?? auth.AccessTokenLifetime);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = auth.Issuer,
            Audience = auth.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = keys.ActiveCredentials,
            Claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["sub"] = user.Value.ToString(),
                ["sid"] = session.Value.ToString(),
                ["rid"] = role.Value.ToString(),
                ["cty"] = clientType,
                ["jti"] = Guid.CreateVersion7().ToString("N", CultureInfo.InvariantCulture),
            },
        };
        return new AccessToken(_handler.CreateToken(descriptor), expires);
    }
}
