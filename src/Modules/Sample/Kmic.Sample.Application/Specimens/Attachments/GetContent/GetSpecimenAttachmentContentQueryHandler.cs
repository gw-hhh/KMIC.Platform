using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Sample.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Application.Specimens.Attachments.GetContent;

/// <summary>
/// 获取示例附件的下载信息
/// </summary>
public sealed class GetSpecimenAttachmentContentQueryHandler(ISampleQueryDb db, IEntityFinder finder, IFileDownloads files)
    : IQueryHandler<GetSpecimenAttachmentContentQuery, FileDownload>
{
    /// <inheritdoc />
    public async ValueTask<Result<FileDownload>> Handle(GetSpecimenAttachmentContentQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // 范围外的示例按不存在处理，访问权限和附件归属确认后再查询下载信息
        var found = await finder.FindByIdOrNotFoundAsync(db.Specimens.Include(s => s.Attachments), query.Id, SampleErrors.NotFound, cancellationToken);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var attachment = found.Value.Attachments.FirstOrDefault(a => a.Id == query.AttachmentId);
        if (attachment is null)
        {
            return SampleErrors.AttachmentNotFound;
        }

        return await files.GetAsync(attachment.FileId.Value, cancellationToken);
    }
}
