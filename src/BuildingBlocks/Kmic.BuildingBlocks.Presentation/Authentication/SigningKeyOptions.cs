namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>单个签名密钥配置</summary>
public sealed class SigningKeyOptions
{
    /// <summary>密钥标识（写入令牌头 kid，轮换时据此匹配验证密钥）</summary>
    public string Kid { get; init; } = "";

    /// <summary>密钥用途</summary>
    public SigningKeyStatus Status { get; init; }

    /// <summary>PEM 私钥文件路径（签发密钥必填，严禁打包进代码或镜像）</summary>
    public string? PrivateKeyPath { get; init; }

    /// <summary>PEM 公钥文件路径（仅验证密钥必填）</summary>
    public string? PublicKeyPath { get; init; }
}
