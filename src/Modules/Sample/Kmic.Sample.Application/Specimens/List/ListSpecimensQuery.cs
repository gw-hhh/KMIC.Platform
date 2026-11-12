using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Sample.Application.Specimens.List;

/// <summary>
/// 查询示例标本列表
/// </summary>
/// <param name="Q">关键字，在编号和标题里模糊匹配</param>
/// <param name="Status">状态编码，可以给多个</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(SamplePermissions.View)]
public sealed record ListSpecimensQuery(string? Q, IReadOnlyList<string>? Status, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<SpecimenListItem>>;
