using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.GetContent;

/// <summary>
/// 下载示例标本的一个附件
/// </summary>
/// <remarks>
/// 下载权限完全继承标本：先按数据范围找到标本、确认附件属于它，再向 Files 取下载信息。每次下载都留审计记录
/// </remarks>
/// <param name="Id">标本</param>
/// <param name="AttachmentId">附件</param>
[RequirePermission(SamplePermissions.View)]
[AuditedRead]
public sealed record GetSpecimenAttachmentContentQuery(SpecimenId Id, SpecimenAttachmentId AttachmentId) : IQuery<FileDownload>;
