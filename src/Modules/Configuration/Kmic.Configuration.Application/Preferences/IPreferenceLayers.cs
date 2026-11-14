using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>
/// 读取用户的分层偏好，系统层和角色层使用缓存，用户层直接读库
/// </summary>
/// <remarks>
/// 由基础设施实现读取和缓存，应用层通过此接口访问
/// </remarks>
public interface IPreferenceLayers
{
    /// <summary>
    /// 取用户在某个角色下的三层偏好值
    /// </summary>
    /// <param name="userId">用户 ID，为空时不读取用户层</param>
    /// <param name="roleId">当前角色 ID，为空时仅读取系统层</param>
    /// <param name="ct">取消令牌</param>
    Task<LayeredPreferences> GetAsync(UserId? userId, RoleId? roleId, CancellationToken ct);
}
