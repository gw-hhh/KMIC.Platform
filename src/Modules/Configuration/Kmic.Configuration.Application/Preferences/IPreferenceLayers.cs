using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>
/// 读一个用户的偏好各层：系统层与角色层走缓存，用户层直接读库
/// </summary>
/// <remarks>
/// 缓存只能在基础设施里碰，所以读取放在这个接口后面
/// </remarks>
public interface IPreferenceLayers
{
    /// <summary>
    /// 取用户在某个角色下的三层偏好值
    /// </summary>
    /// <param name="userId">用户；系统身份没有，没有就没有用户层</param>
    /// <param name="roleId">用户当前使用的角色；没有就只有系统层</param>
    /// <param name="ct">取消令牌</param>
    Task<LayeredPreferences> GetAsync(UserId? userId, RoleId? roleId, CancellationToken ct);
}
