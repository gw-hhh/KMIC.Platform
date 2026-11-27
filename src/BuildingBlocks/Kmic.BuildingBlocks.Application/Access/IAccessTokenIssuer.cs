using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>访问令牌签发器，签发的令牌不包含权限点与数据范围。</summary>
public interface IAccessTokenIssuer
{
    /// <summary>签发访问令牌；未指定 <paramref name="lifetime"/> 时使用配置默认值。</summary>
    AccessToken Issue(UserId user, SessionId session, RoleId role, string clientType, DateTimeOffset now, TimeSpan? lifetime = null);
}
