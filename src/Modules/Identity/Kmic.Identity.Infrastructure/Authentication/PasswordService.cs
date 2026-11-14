using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Kmic.Identity.Infrastructure.Authentication;

/// <summary>
/// 基于 ASP.NET Core 的密码哈希服务
/// </summary>
internal sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();

    // 无账号或未设置密码时也执行哈希校验，减少响应耗时差异
    private readonly string _decoyHash;

    public PasswordService() => _decoyHash = _hasher.HashPassword(null!, "kmic-decoy-password");

    public string Hash(User user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(password);
        return _hasher.HashPassword(user, password);
    }

    public PasswordCheck Verify(User user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(password);

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            Burn(password);
            return PasswordCheck.Failed;
        }

        return _hasher.VerifyHashedPassword(user, user.PasswordHash, password) switch
        {
            PasswordVerificationResult.Success => PasswordCheck.Success,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordCheck.SuccessRehashNeeded,
            _ => PasswordCheck.Failed,
        };
    }

    public void Burn(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        _ = _hasher.VerifyHashedPassword(null!, _decoyHash, password);
    }
}
