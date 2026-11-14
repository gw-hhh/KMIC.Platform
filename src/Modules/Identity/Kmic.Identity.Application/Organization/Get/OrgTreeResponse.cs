using System.Text.Json.Serialization;

namespace Kmic.Identity.Application.Organization.Get;

/// <summary>
/// 组织树
/// </summary>
/// <param name="ConcurrencyVersion">组织树版本号，输出为 version；修改组织树的命令用它做并发检查</param>
/// <param name="Units">全部组织单元的平铺列表，按上级和排序值排列，前端通过 parentId 构建树</param>
public sealed record OrgTreeResponse([property: JsonPropertyName("version")] long ConcurrencyVersion, IReadOnlyList<OrgUnitItem> Units);
