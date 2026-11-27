using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <param name="ExpectedVersion">用户的预期版本号</param>
/// <param name="Anchors">新的组织锚点</param>
public sealed record SetGrantAnchorsRequest(long ExpectedVersion, IReadOnlyList<OrgUnitId> Anchors);
