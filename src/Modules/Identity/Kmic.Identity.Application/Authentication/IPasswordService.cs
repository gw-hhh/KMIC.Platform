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
    /// 做一次与校验相当的哈希计算，结果丢弃：账号不存在时也耗同样的时间，防止从响应时间判断账号是否存在
    /// </summary>
    void Burn(string password);
}
