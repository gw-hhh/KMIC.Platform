using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 替换角色授予组织锚点的请求
/// </summary>
/// <param name="ExpectedVersion">用户的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="Anchors">新的组织锚点，须启用；空列表表示清空锚点，改用用户所属组织</param>
public sealed record SetGrantAnchorsRequest(long ExpectedVersion, IReadOnlyList<OrgUnitId> Anchors);
