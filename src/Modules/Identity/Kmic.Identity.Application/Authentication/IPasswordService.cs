using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 密码哈希与校验
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// 生成哈希
    /// </summary>
    string Hash(User user, string password);

    /// <summary>
    /// 校验密码；通过但哈希参数已过时时，要求调用方用新参数重新保存
    /// </summary>
    PasswordCheck Verify(User user, string password);

    /// <summary>
    /// 账号不存在或未设置密码时，也执行哈希校验，降低按耗时枚举账号的风险
    /// </summary>
    void Burn(string password);
}
