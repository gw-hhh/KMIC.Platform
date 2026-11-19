using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 签发访问令牌
/// </summary>
/// <remarks>
/// 令牌里不放权限和数据范围
/// </remarks>
public interface IAccessTokenIssuer
{
    /// <summary>
    /// 签发一个访问令牌
    /// </summary>
    /// <param name="lifetime">有效期，不传时用配置的默认值</param>
    AccessToken Issue(UserId user, SessionId session, RoleId role, string clientType, DateTimeOffset now, TimeSpan? lifetime = null);
}
