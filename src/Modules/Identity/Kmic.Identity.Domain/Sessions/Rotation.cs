namespace Kmic.Identity.Domain.Sessions;

/// <summary>
/// 一次轮换的结果与新令牌
/// </summary>
public sealed class Rotation
{
    private Rotation(RotationOutcome outcome, RefreshToken? newToken)
    {
        Outcome = outcome;
        NewToken = newToken;
    }

    /// <summary>
    /// 结果
    /// </summary>
    public RotationOutcome Outcome { get; }

    /// <summary>
    /// 轮换成功时的新令牌
    /// </summary>
    public RefreshToken? NewToken { get; }

    internal static Rotation Rotated(RefreshToken newToken) => new(RotationOutcome.Rotated, newToken);

    internal static Rotation Invalid() => new(RotationOutcome.Invalid, null);

    internal static Rotation Reused() => new(RotationOutcome.Reused, null);
}
