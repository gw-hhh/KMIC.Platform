namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>签名密钥用途</summary>
public enum SigningKeyStatus
{
    /// <summary>活跃密钥：同时用于签发与验证（全局唯一）</summary>
    Active,

    /// <summary>过渡密钥：仅用于密钥轮换期间的验签</summary>
    ValidationOnly,
}
