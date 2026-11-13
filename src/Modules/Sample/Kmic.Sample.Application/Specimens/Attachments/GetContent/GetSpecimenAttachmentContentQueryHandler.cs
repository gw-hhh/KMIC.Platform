using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Sample.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Application.Specimens.Attachments.GetContent;

/// <summary>
/// 处理 <see cref="GetSpecimenAttachmentContentQuery"/>
/// </summary>
public sealed class GetSpecimenAttachmentContentQueryHandler(ISampleQueryDb db, IEntityFinder finder, IFileDownloads files)
    : IQueryHandler<GetSpecimenAttachmentContentQuery, FileDownload>
{
    /// <inheritdoc />
    public async ValueTask<Result<FileDownload>> Handle(GetSpecimenAttachmentContentQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // 数据范围外的标本和不存在一样；授权完成之后才能向 Files 取下载信息
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
