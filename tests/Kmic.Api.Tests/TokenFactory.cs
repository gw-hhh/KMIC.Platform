using System.Security.Claims;
using System.Security.Cryptography;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Kmic.Api.Tests;

public sealed class TokenFactory : IDisposable
{
    public const string Issuer = "kmic-api-test";
    public const string Audience = "kmic-api-test-clients";
    private const string Kid = "api-test";

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "kmic-api-tests-" + Guid.CreateVersion7().ToString("N"));
    private readonly RSA _key = RSA.Create(2048);

    public TokenFactory()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(PrivateKeyPath, _key.ExportPkcs8PrivateKeyPem());
    }

    public string PrivateKeyPath => Path.Combine(_directory, Kid + ".pem");

    public string Create(TestIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        var now = TimeProvider.System.GetUtcNow().UtcDateTime;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Subject = new ClaimsIdentity(
            [
                new Claim("sub", identity.UserId.Value.ToString()),
                new Claim("sid", identity.SessionId.Value.ToString()),
                new Claim("rid", identity.RoleId.Value.ToString()),
            ]),
            NotBefore = now.AddMinutes(-1),
            Expires = now.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(_key) { KeyId = Kid }, SecurityAlgorithms.RsaSha256),
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public IReadOnlyDictionary<string, string?> Settings() => new Dictionary<string, string?>(StringComparer.Ordinal)
    {
        ["Kmic:Auth:Issuer"] = Issuer,
        ["Kmic:Auth:Audience"] = Audience,
        ["Kmic:Auth:SigningKeys:0:Kid"] = Kid,
        ["Kmic:Auth:SigningKeys:0:Status"] = "Active",
        ["Kmic:Auth:SigningKeys:0:PrivateKeyPath"] = PrivateKeyPath,
    };

    public void Dispose()
    {
        _key.Dispose();
        Directory.Delete(_directory, recursive: true);
    }
}
