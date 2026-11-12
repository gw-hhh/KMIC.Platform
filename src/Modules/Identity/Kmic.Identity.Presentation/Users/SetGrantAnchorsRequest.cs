using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 替换授予上锚点的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的用户版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="Anchors">新的锚点，必须是启用的组织单元；给空列表表示不设锚点，从用户所属的组织单元算</param>
public sealed record SetGrantAnchorsRequest(long ExpectedVersion, IReadOnlyList<OrgUnitId> Anchors);
