using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.GetContent;

/// <summary>
/// 下载示例的一个附件
/// </summary>
[RequirePermission(SamplePermissions.View)]
[AuditedRead]
public sealed record GetSpecimenAttachmentContentQuery(SpecimenId Id, SpecimenAttachmentId AttachmentId) : IQuery<FileDownload>;
