namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>
/// 签名密钥的用途
/// </summary>
public enum SigningKeyStatus
{
    /// <summary>
    /// 用于签发，同时用于验证；恰好一个
    /// </summary>
    Active,

    /// <summary>
    /// 只用于验证：轮换期间新旧密钥的另一个
    /// </summary>
    ValidationOnly,
}
