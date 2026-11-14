using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Application.Specimens.Shared;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Application.Specimens.GetDetail;

/// <summary>
/// 查询示例详情
/// </summary>
public sealed class GetSpecimenDetailQueryHandler(ISampleQueryDb db, IEntityFinder finder, IAllowedActions<Specimen> allowedActions)
    : IQueryHandler<GetSpecimenDetailQuery, SpecimenDetailResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<SpecimenDetailResponse>> Handle(GetSpecimenDetailQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // 范围外与不存在均返回 NotFound；仅范围外访问记安全事件
        var found = await finder.FindByIdOrNotFoundAsync(db.Specimens.Include(s => s.Items).Include(s => s.Attachments), query.Id, SampleErrors.NotFound, cancellationToken);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var specimen = found.Value;
        return new SpecimenDetailResponse(
            specimen.Id,
            specimen.ConcurrencyVersion,
            specimen.Number,
            specimen.Title,
            specimen.Status,
            [.. specimen.Items.Select(i => new SpecimenItemResponse(i.Name, i.Quantity))],
            [.. specimen.Attachments.Select(a => new SpecimenAttachmentResponse(a.Id.Value, a.FileName, a.Size, a.FileType))],
            allowedActions.For(specimen));
    }
}
