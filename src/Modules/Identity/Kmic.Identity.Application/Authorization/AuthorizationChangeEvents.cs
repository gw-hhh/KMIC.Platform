using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Authorization;

/// <summary>
/// 授权配置变更的安全事件
/// </summary>
/// <remarks>
/// 角色权限、数据范围、启停状态、用户授予及组织树变更除记录审计外，还统一写入授权配置变更安全事件，便于追溯授权来源
/// </remarks>
internal static class AuthorizationChangeEvents
{
    /// <summary>
    /// 变更成功时记录安全事件并返回版本响应，失败时原样返回错误
    /// </summary>
    public static Result<VersionedResponse<TId>> Complete<TId>(ISecurityEventWriter writer, Result done, string resourceType, TId id, string change, IReadOnlyDictionary<string, string>? details = null)
        where TId : struct, IStronglyTypedId
    {
        if (done.IsFailure)
        {
            return done.Error;
        }

        Write(writer, resourceType, id.Value, change, details);
        return new VersionedResponse<TId>(id);
    }

    /// <summary>
    /// 记录授权配置变更，change 为变更类型编码，如 PermissionsSet
    /// </summary>
    public static void Write(ISecurityEventWriter writer, string resourceType, Guid resourceId, string change, IReadOnlyDictionary<string, string>? details = null)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(new SecurityEvent("AuthorizationConfigChanged", SecuritySeverity.Info, change)
        {
            ResourceType = resourceType,
            ResourceId = resourceId,
            Details = details,
        });
    }
}
