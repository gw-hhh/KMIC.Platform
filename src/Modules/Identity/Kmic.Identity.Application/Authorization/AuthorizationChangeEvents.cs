using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Authorization;

// 角色、授予及组织树变更除审计外均须记录授权配置变更安全事件
internal static class AuthorizationChangeEvents
{
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

    // change 为变更类型编码（如 PermissionsSet）
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
