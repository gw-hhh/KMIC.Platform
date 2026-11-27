namespace Kmic.Identity.Application.Authentication;

/// <summary>密码校验结果</summary>
public enum PasswordCheck
{
    Failed,
    Success,

    /// <summary>密码匹配，但哈希参数已过时，须重新计算并保存哈希</summary>
    SuccessRehashNeeded,
}
