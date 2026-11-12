using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.Specimens;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Application.ContractQueries;

/// <summary>
/// 处理 <see cref="GetSpecimenReferencesQuery"/>
/// </summary>
[BypassesDataScope("引用解析只返回名称和状态，不受调用方数据范围限制：解析的是调用方已经保存下来的引用")]
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

        // 已关闭的标本不能再被选用，对引用方来说就是“停用”
        return rows.ToDictionary(
            r => r.Id.Value,
            r => new ReferenceInfo(r.Id.Value, r.Title, r.Number, r.Status == SpecimenStatus.Closed ? ReferenceStatus.Disabled : ReferenceStatus.Active));
    }
}
