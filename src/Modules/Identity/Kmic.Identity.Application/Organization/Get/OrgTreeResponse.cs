using System.Text.Json.Serialization;

namespace Kmic.Identity.Application.Organization.Get;

/// <summary>
/// 组织树
/// </summary>
/// <param name="ConcurrencyVersion">组织树版本号</param>
/// <param name="Units">全部组织单元的平铺列表</param>
public sealed record OrgTreeResponse([property: JsonPropertyName("version")] long ConcurrencyVersion, IReadOnlyList<OrgUnitItem> Units);
