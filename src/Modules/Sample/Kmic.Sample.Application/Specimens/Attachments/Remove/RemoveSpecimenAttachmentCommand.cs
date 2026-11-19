using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.Remove;

/// <summary>
/// 移除示例的附件
/// </summary>
/// <param name="Id">示例 ID</param>
/// <param name="AttachmentId">附件 ID</param>
/// <param name="ExpectedVersion">读取时的版本号</param>
[RequirePermission(SamplePermissions.Edit)]
[Audited]
public sealed record RemoveSpecimenAttachmentCommand(SpecimenId Id, SpecimenAttachmentId AttachmentId, long ExpectedVersion) : ICommand<VersionedResponse<SpecimenId>>;
