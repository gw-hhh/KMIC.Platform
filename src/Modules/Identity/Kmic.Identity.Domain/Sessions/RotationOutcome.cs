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
    /// 旧令牌在宽限期外重用；会话作废尚需提交事务
    /// </summary>
    Reused,
}
