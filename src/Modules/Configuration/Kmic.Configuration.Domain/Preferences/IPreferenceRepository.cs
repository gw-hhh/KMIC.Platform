using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Configuration.Domain.Preferences;

/// <summary>
/// 偏好值的仓储
/// </summary>
public interface IPreferenceRepository : IRepository<PreferenceValue, PreferenceValueId>
{
    /// <summary>
    /// 读取指定层的偏好值，供修改；不存在时返回 null
    /// </summary>
    /// <remarks>
    /// 角色层和用户层须提供 roleId，用户层还须提供 userId；其余归属参数为空
    /// </remarks>
    Task<PreferenceValue?> FindForUpdateAsync(string key, PreferenceLayer layer, RoleId? roleId, UserId? userId, CancellationToken ct);
}
