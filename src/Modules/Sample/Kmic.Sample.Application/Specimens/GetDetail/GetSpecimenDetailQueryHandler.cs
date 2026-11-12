using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Application.Specimens.Shared;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Application.Specimens.GetDetail;

/// <summary>
/// 处理 <see cref="GetSpecimenDetailQuery"/>
/// </summary>
public sealed class GetSpecimenDetailQueryHandler(ISampleQueryDb db, IAllowedActions<Specimen> allowedActions)
    : IQueryHandler<GetSpecimenDetailQuery, SpecimenDetailResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<SpecimenDetailResponse>> Handle(GetSpecimenDetailQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // 查询入口已经带了数据范围过滤：范围外的数据和不存在一样，返回 NotFound
        var specimen = await db.Specimens.Include(s => s.Items).SingleOrDefaultAsync(s => s.Id == query.Id, cancellationToken);
        if (specimen is null)
        {
            return SampleErrors.NotFound;
        }

        return new SpecimenDetailResponse(
            specimen.Id,
            specimen.ConcurrencyVersion,
            specimen.Number,
            specimen.Title,
            specimen.Status,
            [.. specimen.Items.Select(i => new SpecimenItemResponse(i.Name, i.Quantity))],
            allowedActions.For(specimen));
    }
}
