namespace Kmic.Identity.Application.Authentication;

/// <summary>常见弱密码清单</summary>
public interface IWeakPasswordList
{
    /// <summary>判断是否命中弱密码（不区分大小写）</summary>
    bool Contains(string password);
}
