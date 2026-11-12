using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.ResetPassword;

/// <summary>
/// 重置密码的结果
/// </summary>
/// <param name="Id">用户</param>
/// <param name="TemporaryPassword">一次性临时密码，只在这次响应里出现，不写日志、不进审计内容</param>
public sealed record ResetUserPasswordResponse(UserId Id, string TemporaryPassword)
{
    /// <summary>
    /// 日志和异常里打印这个对象时不带出密码
    /// </summary>
    public override string ToString() => $"{nameof(ResetUserPasswordResponse)} {{ Id = {Id} }}";
}
