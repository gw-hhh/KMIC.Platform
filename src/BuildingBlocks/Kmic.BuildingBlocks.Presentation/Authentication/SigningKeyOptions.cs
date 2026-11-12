namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>
/// 一个签名密钥的配置
/// </summary>
public sealed class SigningKeyOptions
{
    /// <summary>
    /// 密钥标识，写在令牌头的 kid 里，轮换时按它找到验证用的密钥
    /// </summary>
    public string Kid { get; init; } = "";

    /// <summary>
    /// 用途
    /// </summary>
    public SigningKeyStatus Status { get; init; }

    /// <summary>
    /// 私钥文件（PEM），用于签发的密钥必须有；文件放在密钥管理提供的位置，不进代码和镜像
    /// </summary>
    public string? PrivateKeyPath { get; init; }

    /// <summary>
    /// 公钥文件（PEM），只用于验证的密钥必须有
    /// </summary>
    public string? PublicKeyPath { get; init; }
}
