using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Authorization;

/// <summary>
/// 授权配置变更的安全事件
/// </summary>
/// <remarks>
/// 角色的权限点与数据范围、停用启用、用户的角色授予与锚点、组织树的变动，都会改变谁能做什么，
/// 在审计之外再记一条安全事件，查“权限是怎么变成这样的”时按类型就能找到
/// </remarks>
internal static class AuthorizationChangeEvents
{
    /// <summary>
    /// 命令的收尾：改动成功时记一条变更事件并返回新版本，失败时原样返回错误
    /// </summary>
    /// <param name="writer">安全事件写入器</param>
    /// <param name="done">领域方法的结果</param>
    /// <param name="resourceType">被改的资源类型</param>
    /// <param name="id">资源 ID</param>
    /// <param name="change">改了什么</param>
    /// <typeparam name="TId">资源 ID 类型</typeparam>
    public static Result<VersionedResponse<TId>> Complete<TId>(ISecurityEventWriter writer, Result done, string resourceType, TId id, string change)
        where TId : struct, IStronglyTypedId
    {
        if (done.IsFailure)
        {
            return done.Error;
        }

        Write(writer, resourceType, id.Value, change);
        return new VersionedResponse<TId>(id);
    }

    /// <summary>
    /// 记一条授权配置变更
    /// </summary>
    /// <param name="writer">安全事件写入器</param>
    /// <param name="resourceType">被改的资源类型，如 Role、User、OrgUnit</param>
    /// <param name="resourceId">资源 ID</param>
    /// <param name="change">改了什么，如 PermissionsSet</param>
    public static void Write(ISecurityEventWriter writer, string resourceType, Guid resourceId, string change)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(new SecurityEvent("AuthorizationConfigChanged", SecuritySeverity.Info, change)
        {
            ResourceType = resourceType,
            ResourceId = resourceId,
        });
    }
}
