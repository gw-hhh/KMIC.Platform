using System.Security.Claims;
using System.Security.Cryptography;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

/// <summary>
/// 生成测试用的 RSA 密钥（PEM 文件）和访问令牌
/// </summary>
internal sealed class TokenFactory : IDisposable
{
    public const string Issuer = "kmic-test";
    public const string Audience = "kmic-test-api";

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "kmic-presentation-tests-" + Guid.CreateVersion7().ToString("N"));
    private readonly Dictionary<string, RSA> _keys = new(StringComparer.Ordinal);

    public TokenFactory() => Directory.CreateDirectory(_directory);

    /// <summary>
    /// 新建一个密钥，返回它的私钥文件和公钥文件
    /// </summary>
    public (string PrivatePath, string PublicPath) AddKey(string kid, int size = 2048)
    {
        var rsa = RSA.Create(size);
        _keys[kid] = rsa;
        var privatePath = Path.Combine(_directory, kid + ".pem");
        var publicPath = Path.Combine(_directory, kid + ".pub.pem");
        File.WriteAllText(privatePath, rsa.ExportPkcs8PrivateKeyPem());
        File.WriteAllText(publicPath, rsa.ExportSubjectPublicKeyInfoPem());
        return (privatePath, publicPath);
    }

    /// <summary>
    /// 配置：一个 Active 密钥，可选一个 ValidationOnly 密钥
    /// </summary>
    public Dictionary<string, string?> Configuration(string activeKid, string? validationOnlyKid = null)
    {
        var configuration = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Kmic:Auth:Issuer"] = Issuer,
            ["Kmic:Auth:Audience"] = Audience,
            ["Kmic:Auth:SigningKeys:0:Kid"] = activeKid,
            ["Kmic:Auth:SigningKeys:0:Status"] = "Active",
            ["Kmic:Auth:SigningKeys:0:PrivateKeyPath"] = Path.Combine(_directory, activeKid + ".pem"),
        };
        if (validationOnlyKid is not null)
        {
            configuration["Kmic:Auth:SigningKeys:1:Kid"] = validationOnlyKid;
            configuration["Kmic:Auth:SigningKeys:1:Status"] = "ValidationOnly";
            configuration["Kmic:Auth:SigningKeys:1:PublicKeyPath"] = Path.Combine(_directory, validationOnlyKid + ".pub.pem");
        }

        return configuration;
    }

    /// <summary>
    /// 签发令牌：默认值是一个合法的、刚签发的令牌，测试改其中要检验的那一项
    /// </summary>
    public string Create(
        TestIdentity identity,
        string kid,
        string? issuer = Issuer,
        string? audience = Audience,
        TimeSpan? expiresIn = null,
        bool includeSession = true,
        bool includeRole = true,
        string algorithm = SecurityAlgorithms.RsaSha256,
        RSA? signWith = null)
    {
        var now = TimeProvider.System.GetUtcNow().UtcDateTime;
        var claims = new List<Claim> { new("sub", identity.UserId.Value.ToString()) };
        if (includeSession)
        {
            claims.Add(new Claim("sid", identity.SessionId.Value.ToString()));
        }

        if (includeRole)
        {
            claims.Add(new Claim("rid", identity.RoleId.Value.ToString()));
        }

        var key = new RsaSecurityKey(signWith ?? _keys[kid]) { KeyId = kid };
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = now.AddHours(-3),
            IssuedAt = now.AddHours(-3),
            Expires = now + (expiresIn ?? TimeSpan.FromMinutes(15)),
            SigningCredentials = new SigningCredentials(key, algorithm),
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    /// <summary>
    /// 用对称密钥签发（HS256）：验证必须拒绝
    /// </summary>
    public static string CreateSymmetric(TestIdentity identity)
    {
        var secret = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(64)) { KeyId = "symmetric" };
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Subject = new ClaimsIdentity([new Claim("sub", identity.UserId.Value.ToString()), new Claim("sid", identity.SessionId.Value.ToString()), new Claim("rid", identity.RoleId.Value.ToString())]),
            Expires = TimeProvider.System.GetUtcNow().UtcDateTime.AddMinutes(15),
            SigningCredentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256),
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public void Dispose()
    {
        foreach (var key in _keys.Values)
        {
            key.Dispose();
        }

        Directory.Delete(_directory, recursive: true);
    }
}
