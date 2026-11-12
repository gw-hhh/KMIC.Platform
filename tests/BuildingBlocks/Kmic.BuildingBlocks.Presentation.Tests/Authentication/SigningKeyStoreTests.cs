using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Presentation.Authentication;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.BuildingBlocks.Presentation.Tests.Authentication;

/// <summary>
/// 签名密钥的启动校验：恰好一个 Active、RSA 不小于 2048、文件存在且是有效的 PEM
/// </summary>
public sealed class SigningKeyStoreTests : IDisposable
{
    private readonly TokenFactory _tokens = new();

    public void Dispose() => _tokens.Dispose();

    [Fact]
    public void ValidConfiguration_LoadsTheActiveKeyAndAllValidationKeys()
    {
        _tokens.AddKey("2026-11");
        _tokens.AddKey("2026-05");

        using var store = Load(_tokens.Configuration("2026-11", "2026-05"));

        store.ActiveCredentials.Key.KeyId.ShouldBe("2026-11");
        store.ActiveCredentials.Algorithm.ShouldBe("RS256");
        store.ValidationKeys.Select(k => k.KeyId).ShouldBe(["2026-11", "2026-05"]);
    }

    [Fact]
    public void NoActiveKey_IsRejected()
    {
        _tokens.AddKey("2026-05");
        var configuration = _tokens.Configuration("2026-05");
        configuration["Kmic:Auth:SigningKeys:0:Status"] = "ValidationOnly";
        configuration["Kmic:Auth:SigningKeys:0:PublicKeyPath"] = configuration["Kmic:Auth:SigningKeys:0:PrivateKeyPath"]!.Replace(".pem", ".pub.pem", StringComparison.Ordinal);

        Problems(configuration).ShouldContain(p => p.Contains("恰好有一个 Active", StringComparison.Ordinal) && p.Contains("现在有 0 个", StringComparison.Ordinal));
    }

    [Fact]
    public void TwoActiveKeys_AreRejected()
    {
        _tokens.AddKey("a");
        _tokens.AddKey("b");
        var configuration = _tokens.Configuration("a");
        configuration["Kmic:Auth:SigningKeys:1:Kid"] = "b";
        configuration["Kmic:Auth:SigningKeys:1:Status"] = "Active";
        configuration["Kmic:Auth:SigningKeys:1:PrivateKeyPath"] = configuration["Kmic:Auth:SigningKeys:0:PrivateKeyPath"]!.Replace("a.pem", "b.pem", StringComparison.Ordinal);

        Problems(configuration).ShouldContain(p => p.Contains("现在有 2 个", StringComparison.Ordinal));
    }

    [Fact]
    public void DuplicateKeyId_IsRejected()
    {
        _tokens.AddKey("a");
        _tokens.AddKey("b");
        var configuration = _tokens.Configuration("a", "b");
        configuration["Kmic:Auth:SigningKeys:1:Kid"] = "a";

        Problems(configuration).ShouldContain(p => p.Contains("标识 a 重复", StringComparison.Ordinal));
    }

    [Fact]
    public void KeyShorterThan2048Bits_IsRejected()
    {
        _tokens.AddKey("weak", size: 1024);

        Problems(_tokens.Configuration("weak")).ShouldContain(p => p.Contains("RSA 长度 1024", StringComparison.Ordinal));
    }

    [Fact]
    public void ActiveKeyWithoutAPrivateKeyFile_IsRejected()
    {
        _tokens.AddKey("a");
        var configuration = _tokens.Configuration("a");
        configuration.Remove("Kmic:Auth:SigningKeys:0:PrivateKeyPath");

        Problems(configuration).ShouldContain(p => p.Contains("要配置私钥文件", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidationKeyWithoutAPublicKeyFile_IsRejected()
    {
        _tokens.AddKey("a");
        _tokens.AddKey("b");
        var configuration = _tokens.Configuration("a", "b");
        configuration.Remove("Kmic:Auth:SigningKeys:1:PublicKeyPath");

        Problems(configuration).ShouldContain(p => p.Contains("要配置公钥文件", StringComparison.Ordinal));
    }

    [Fact]
    public void MissingFile_IsRejected()
    {
        _tokens.AddKey("a");
        var configuration = _tokens.Configuration("a");
        configuration["Kmic:Auth:SigningKeys:0:PrivateKeyPath"] += ".missing";

        Problems(configuration).ShouldContain(p => p.Contains("找不到文件", StringComparison.Ordinal));
    }

    [Fact]
    public async Task FileThatIsNotAPem_IsRejected()
    {
        _tokens.AddKey("a");
        var configuration = _tokens.Configuration("a");
        await File.WriteAllTextAsync(configuration["Kmic:Auth:SigningKeys:0:PrivateKeyPath"]!, "not a key", TestContext.Current.CancellationToken);

        Problems(configuration).ShouldContain(p => p.Contains("不是有效的 RSA PEM", StringComparison.Ordinal));
    }

    [Fact]
    public void AllProblemsAreReportedTogether()
    {
        var configuration = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Kmic:Auth:SigningKeys:0:Kid"] = "a",
            ["Kmic:Auth:SigningKeys:0:Status"] = "ValidationOnly",
        };

        Problems(configuration).Count.ShouldBe(2);
    }

    [Fact]
    public async Task HostWithAnInvalidKeyConfiguration_RefusesToStart()
    {
        var configuration = _tokens.Configuration("missing");
        var host = ApiHost.Create([new ListedModule("Samples", AuthEndpoints.All)], configuration: configuration, jwt: true, skipValidation: true);
        await using var _ = host;

        await Should.ThrowAsync<DeclarationException>(host.StartAsync());
    }

    [Fact]
    public async Task HostWithoutIssuer_RefusesToStart()
    {
        _tokens.AddKey("a");
        var configuration = _tokens.Configuration("a");
        configuration.Remove("Kmic:Auth:Issuer");
        var host = ApiHost.Create([new ListedModule("Samples", AuthEndpoints.All)], configuration: configuration, jwt: true, skipValidation: true);
        await using var _ = host;

        await Should.ThrowAsync<OptionsValidationException>(host.StartAsync());
    }

    private static SigningKeyStore Load(Dictionary<string, string?> configuration) => new(Options.Create(Bind(configuration)));

    private static List<string> Problems(Dictionary<string, string?> configuration)
    {
        var thrown = Should.Throw<DeclarationException>(() => Load(configuration));
        return [.. thrown.Problems];
    }

    private static KmicAuthOptions Bind(Dictionary<string, string?> configuration)
    {
        var options = new ConfigurationBuilder().AddInMemoryCollection(configuration).Build()
            .GetSection(KmicAuthOptions.Section).Get<KmicAuthOptions>();
        return options ?? new KmicAuthOptions();
    }
}
