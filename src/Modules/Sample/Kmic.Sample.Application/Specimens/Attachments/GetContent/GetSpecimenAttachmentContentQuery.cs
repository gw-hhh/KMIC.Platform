using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.GetContent;

/// <summary>
/// 下载示例的一个附件
/// </summary>
/// <remarks>
/// 先校验示例的访问权限并确认附件归属，再查询文件下载信息。每次下载均记录审计日志
/// </remarks>
[RequirePermission(SamplePermissions.View)]
[AuditedRead]
public sealed record GetSpecimenAttachmentContentQuery(SpecimenId Id, SpecimenAttachmentId AttachmentId) : IQuery<FileDownload>;
