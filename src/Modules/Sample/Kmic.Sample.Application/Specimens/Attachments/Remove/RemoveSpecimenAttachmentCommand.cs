using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.Remove;

/// <summary>
/// 移除示例标本的一个附件：同一个事务里解除文件的关联，文件进入保留期，期间可以重新关联
/// </summary>
/// <param name="Id">标本</param>
/// <param name="AttachmentId">附件</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
[RequirePermission(SamplePermissions.Edit)]
[Audited]
public sealed record RemoveSpecimenAttachmentCommand(SpecimenId Id, SpecimenAttachmentId AttachmentId, long ExpectedVersion) : ICommand<VersionedResponse<SpecimenId>>;
