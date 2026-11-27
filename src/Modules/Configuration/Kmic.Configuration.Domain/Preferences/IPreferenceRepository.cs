using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Configuration.Domain.Preferences;

public interface IPreferenceRepository : IRepository<PreferenceValue, PreferenceValueId>
{
    /// <summary>读取指定层级的偏好值（角色层须传 roleId，用户层须同时传 roleId 与 userId）</summary>
    Task<PreferenceValue?> FindForUpdateAsync(string key, PreferenceLayer layer, RoleId? roleId, UserId? userId, CancellationToken ct);
}
