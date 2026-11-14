using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.Specimens;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Application.ContractQueries;

/// <summary>
/// 批量查询示例的引用信息
/// </summary>
[BypassesDataScope("解析调用方已保存的引用，仅返回 ID、编号、名称和状态，不受调用方数据范围限制")]
public sealed class GetSpecimenReferencesQueryHandler(ISampleQueryDb db)
    : IQueryHandler<GetSpecimenReferencesQuery, IReadOnlyDictionary<Guid, ReferenceInfo>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyDictionary<Guid, ReferenceInfo>>> Handle(GetSpecimenReferencesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await db.Specimens
            .WithoutDataScope()
            .Where(s => query.Ids.Contains(s.Id))
            .Select(s => new { s.Id, s.Number, s.Title, s.Status })
            .ToListAsync(cancellationToken);

        // 已关闭的示例对引用方显示为“停用”
        return rows.ToDictionary(
            r => r.Id.Value,
            r => new ReferenceInfo(r.Id.Value, r.Title, r.Number, r.Status == SpecimenStatus.Closed ? ReferenceStatus.Disabled : ReferenceStatus.Active));
    }
}
