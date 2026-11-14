namespace Kmic.Identity.Domain.Sessions;

/// <summary>
/// 刷新令牌轮换的结果
/// </summary>
public enum RotationOutcome
{
    /// <summary>
    /// 轮换成功，签发了新令牌
    /// </summary>
    Rotated,

    /// <summary>
    /// 令牌或会话不符合刷新条件
    /// </summary>
    Invalid,

    /// <summary>
    /// 旧令牌在宽限期之外再次出现，判定为重用，整个会话已被作废
    /// </summary>
    Reused,
}
