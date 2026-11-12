using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Configuration.Domain.Preferences;

/// <summary>
/// 偏好值的仓储
/// </summary>
public interface IPreferenceRepository : IRepository<PreferenceValue, PreferenceValueId>
{
    /// <summary>
    /// 取某一层上这个键的值行，没有就返回空：调用方据此新增或覆盖
    /// </summary>
    /// <param name="key">偏好键</param>
    /// <param name="layer">层</param>
    /// <param name="roleId">角色层和用户层要给；系统层不给</param>
    /// <param name="userId">用户层要给；其余层不给</param>
    /// <param name="ct">取消令牌</param>
    Task<PreferenceValue?> FindForUpdateAsync(string key, PreferenceLayer layer, RoleId? roleId, UserId? userId, CancellationToken ct);
}
