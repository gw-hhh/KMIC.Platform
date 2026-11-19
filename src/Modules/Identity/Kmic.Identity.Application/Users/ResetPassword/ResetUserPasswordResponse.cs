using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.ResetPassword;

/// <summary>
/// 重置密码的结果
/// </summary>
/// <param name="Id">用户 ID</param>
/// <param name="TemporaryPassword">临时密码</param>
public sealed record ResetUserPasswordResponse(UserId Id, string TemporaryPassword)
{
    /// <summary>
    /// 格式化输出时隐藏临时密码
    /// </summary>
    public override string ToString() => $"{nameof(ResetUserPasswordResponse)} {{ Id = {Id} }}";
}
