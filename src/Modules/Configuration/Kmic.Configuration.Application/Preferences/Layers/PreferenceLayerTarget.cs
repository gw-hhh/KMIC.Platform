using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Identity.Contracts;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 管理命令要改的一层：系统层，或某个角色层
/// </summary>
internal readonly record struct PreferenceLayerTarget(PreferenceLayer Layer, RoleId? RoleId)
{
    /// <summary>
    /// 失效这一层的缓存标签
    /// </summary>
    public string CacheTag => RoleId is { } role ? ConfigurationCacheKeys.PreferencesRole(role) : ConfigurationCacheKeys.PreferencesSystem;

    /// <summary>
    /// 不给角色是系统层，给了是角色层；角色必须存在（已停用的也算，停用的角色可能重新启用）
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
    /// 调用方看到的版本与现在的行是否一致：没看到值（空）要求现在也没有，看到了某个版本要求现在还是它
    /// </summary>
    public static Result CheckVersion(PreferenceValue? row, long? expectedVersion) =>
        (row, expectedVersion) switch
        {
            (null, null) => Result.Success(),
            ({ } existing, { } expected) when existing.ConcurrencyVersion == expected => Result.Success(),
            _ => CommonErrors.VersionConflict,
        };
}
