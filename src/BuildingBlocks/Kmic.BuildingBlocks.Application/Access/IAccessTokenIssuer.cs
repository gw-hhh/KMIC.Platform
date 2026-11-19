using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 签发访问令牌
/// </summary>
/// <remarks>
/// 令牌里只有用户、会话、当前角色和客户端类型，不放权限和数据范围：它们每个请求从服务端加载，变化立即生效。
/// 实现在接口层，和验证用同一套配置
/// </remarks>
public interface IAccessTokenIssuer
{
    /// <summary>
    /// 签发一个访问令牌
    /// </summary>
    /// <param name="lifetime">有效期，不传时用配置的默认值</param>
    AccessToken Issue(UserId user, SessionId session, RoleId role, string clientType, DateTimeOffset now, TimeSpan? lifetime = null);
}
