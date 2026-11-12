namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 密码校验的结果
/// </summary>
public enum PasswordCheck
{
    /// <summary>
    /// 密码不对
    /// </summary>
    Failed,

    /// <summary>
    /// 密码对
    /// </summary>
    Success,

    /// <summary>
    /// 密码对，但哈希参数已经过时，应当用新参数重新哈希并保存
    /// </summary>
    SuccessRehashNeeded,
}
