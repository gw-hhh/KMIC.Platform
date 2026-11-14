namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 密码校验的结果
/// </summary>
public enum PasswordCheck
{
    /// <summary>
    /// 密码不匹配
    /// </summary>
    Failed,

    /// <summary>
    /// 密码匹配
    /// </summary>
    Success,

    /// <summary>
    /// 密码匹配，但哈希参数已过时，须重新计算并保存哈希
    /// </summary>
    SuccessRehashNeeded,
}
