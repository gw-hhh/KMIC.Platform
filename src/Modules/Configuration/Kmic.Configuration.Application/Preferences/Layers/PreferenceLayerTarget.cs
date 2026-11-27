using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Identity.Contracts;

namespace Kmic.Configuration.Application.Preferences.Layers;

// 管理系统层或指定角色层偏好的目标对象
internal readonly record struct PreferenceLayerTarget(PreferenceLayer Layer, RoleId? RoleId)
{
    public string CacheTag => RoleId is { } role ? ConfigurationCacheKeys.PreferencesRole(role) : ConfigurationCacheKeys.PreferencesSystem;

    // 未传角色时定位系统层，否则定位角色层；角色须存在（允许已停用角色）
    public static async Task<Result<PreferenceLayerTarget>> ResolveAsync(RoleId? roleId, PreferenceSubject subject, IRoleReferences roles, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(roles);

        var target = new PreferenceLayerTarget(roleId is null ? PreferenceLayer.System : PreferenceLayer.Role, roleId);
        var allowed = subject.CheckLayer(target.Layer);
        if (allowed.IsFailure)
        {
            return allowed.Error;
        }

        if (roleId is { } role && !(await roles.ResolveAsync([role.Value], ct)).ContainsKey(role.Value))
        {
            return ConfigurationErrors.PreferenceRoleNotFound;
        }

        return target;
    }

    // 未指定预期版本时要求当前尚未保存记录；指定版本时须与现有记录版本一致
    public static Result CheckVersion(PreferenceValue? row, long? expectedVersion) =>
        (row, expectedVersion) switch
        {
            (null, null) => Result.Success(),
            ({ } existing, { } expected) when existing.ConcurrencyVersion == expected => Result.Success(),
            _ => CommonErrors.VersionConflict,
        };
}
