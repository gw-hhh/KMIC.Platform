using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Identity.Contracts;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 管理命令操作的系统层或指定角色层偏好
/// </summary>
internal readonly record struct PreferenceLayerTarget(PreferenceLayer Layer, RoleId? RoleId)
{
    /// <summary>
    /// 当前层级的缓存失效标签
    /// </summary>
    public string CacheTag => RoleId is { } role ? ConfigurationCacheKeys.PreferencesRole(role) : ConfigurationCacheKeys.PreferencesSystem;

    /// <summary>
    /// 未指定角色时选择系统层，否则选择角色层；角色须存在，允许已停用角色
    /// </summary>
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

    /// <summary>
    /// 校验预期版本；未指定版本时要求当前无值，指定版本时要求当前记录版本一致
    /// </summary>
    public static Result CheckVersion(PreferenceValue? row, long? expectedVersion) =>
        (row, expectedVersion) switch
        {
            (null, null) => Result.Success(),
            ({ } existing, { } expected) when existing.ConcurrencyVersion == expected => Result.Success(),
            _ => CommonErrors.VersionConflict,
        };
}
