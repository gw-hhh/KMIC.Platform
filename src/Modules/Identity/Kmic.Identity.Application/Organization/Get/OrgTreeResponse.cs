using System.Text.Json.Serialization;

namespace Kmic.Identity.Application.Organization.Get;

/// <summary>
/// 组织树
/// </summary>
/// <param name="ConcurrencyVersion">组织树版本号，输出为 version；修改组织树的命令用它做并发检查</param>
/// <param name="Units">全部单元，平铺；按上级分组、同一上级下按次序排好，前端按 parentId 拼成树</param>
public sealed record OrgTreeResponse([property: JsonPropertyName("version")] long ConcurrencyVersion, IReadOnlyList<OrgUnitItem> Units);
