namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 常见弱密码清单
/// </summary>
public interface IWeakPasswordList
{
    /// <summary>
    /// 是不是常见弱密码，不区分大小写
    /// </summary>
    bool Contains(string password);
}
