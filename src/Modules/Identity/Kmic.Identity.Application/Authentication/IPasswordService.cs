using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Authentication;

/// <summary>密码哈希与校验</summary>
public interface IPasswordService
{
    /// <summary>生成密码哈希</summary>
    string Hash(User user, string password);

    /// <summary>校验密码；哈希参数过时时提示调用方升级重算</summary>
    PasswordCheck Verify(User user, string password);

    /// <summary>账号不存在或无密码时执行等效哈希校验，防止响应耗时泄露账号存在性</summary>
    void Burn(string password);
}
