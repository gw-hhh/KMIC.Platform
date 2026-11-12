using System.Security.Cryptography;
using Kmic.BuildingBlocks.Application;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>
/// 加载好的签名密钥
/// </summary>
/// <remarks>
/// 验证令牌用全部密钥，签发令牌（Identity 模块）用 <see cref="ActiveCredentials"/>。
/// 算法固定 RS256，密钥长度不得小于 <see cref="MinimumKeySize"/>；启动时加载，有问题拒绝启动
/// </remarks>
public sealed class SigningKeyStore : IDisposable
{
    /// <summary>
    /// RSA 密钥的最小长度
    /// </summary>
    public const int MinimumKeySize = 2048;

    private readonly List<RSA> _keys = [];

    /// <summary>
    /// 按配置加载，全部问题汇总后一次报出
    /// </summary>
    public SigningKeyStore(IOptions<KmicAuthOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var configured = options.Value.SigningKeys;
        var problems = new List<string>();
        var active = configured.Where(k => k.Status == SigningKeyStatus.Active).ToList();
        if (active.Count != 1)
        {
            problems.Add($"必须恰好有一个 Active 的签名密钥，现在有 {active.Count} 个。");
        }

        problems.AddRange(configured.GroupBy(k => k.Kid, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => $"签名密钥标识 {g.Key} 重复。"));

        var validation = new List<SecurityKey>();
        foreach (var key in configured)
        {
            if (Load(key, problems) is { } rsa)
            {
                validation.Add(new RsaSecurityKey(rsa) { KeyId = key.Kid });
                if (key.Status == SigningKeyStatus.Active)
                {
                    ActiveCredentials = new SigningCredentials(new RsaSecurityKey(rsa) { KeyId = key.Kid }, SecurityAlgorithms.RsaSha256);
                }
            }
        }

        if (problems.Count > 0)
        {
            throw new DeclarationException("签名密钥配置", problems);
        }

        ValidationKeys = validation;
    }

    /// <summary>
    /// 签发用的凭据
    /// </summary>
    public SigningCredentials ActiveCredentials { get; } = null!;

    /// <summary>
    /// 验证用的全部密钥，含正在使用的
    /// </summary>
    public IReadOnlyList<SecurityKey> ValidationKeys { get; } = [];

    /// <inheritdoc />
    public void Dispose() => _keys.ForEach(k => k.Dispose());

    private RSA? Load(SigningKeyOptions key, List<string> problems)
    {
        var name = $"签名密钥 {key.Kid}";
        if (string.IsNullOrWhiteSpace(key.Kid))
        {
            problems.Add("有签名密钥没有配置标识（Kid）。");
            return null;
        }

        var path = key.Status == SigningKeyStatus.Active ? key.PrivateKeyPath : key.PublicKeyPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            problems.Add($"{name}：{(key.Status == SigningKeyStatus.Active ? "用于签发的密钥要配置私钥文件（PrivateKeyPath）" : "只用于验证的密钥要配置公钥文件（PublicKeyPath）")}。");
            return null;
        }

        if (!File.Exists(path))
        {
            problems.Add($"{name}：找不到文件 {path}。");
            return null;
        }

        var rsa = RSA.Create();
        try
        {
            rsa.ImportFromPem(File.ReadAllText(path));
        }
        catch (Exception exception) when (exception is ArgumentException or CryptographicException)
        {
            rsa.Dispose();
            problems.Add($"{name}：{path} 不是有效的 RSA PEM 文件。");
            return null;
        }

        if (rsa.KeySize < MinimumKeySize)
        {
            problems.Add($"{name}：RSA 长度 {rsa.KeySize} 小于 {MinimumKeySize}。");
            rsa.Dispose();
            return null;
        }

        _keys.Add(rsa);
        return rsa;
    }
}
