namespace Kmic.Identity.Domain.Sessions;

public enum RotationOutcome
{
    /// <summary>轮换成功，已签发新令牌。</summary>
    Rotated,

    /// <summary>令牌或会话不符合刷新条件。</summary>
    Invalid,

    /// <summary>旧令牌在宽限期外重用；会话作废须待事务提交后生效。</summary>
    Reused,
}
