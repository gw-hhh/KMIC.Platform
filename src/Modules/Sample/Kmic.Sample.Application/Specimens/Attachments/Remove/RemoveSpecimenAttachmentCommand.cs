using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.Remove;

/// <summary>
/// 移除示例的附件
/// </summary>
/// <remarks>
/// 删除附件与发送解除关联请求在同一个事务中完成。
/// 文件的全部引用解除后进入保留期，保留期间可以重新关联
/// </remarks>
/// <param name="Id">示例 ID</param>
/// <param name="AttachmentId">附件 ID</param>
/// <param name="ExpectedVersion">读取时的版本号，与当前版本不一致时返回版本冲突</param>
[RequirePermission(SamplePermissions.Edit)]
[Audited]
public sealed record RemoveSpecimenAttachmentCommand(SpecimenId Id, SpecimenAttachmentId AttachmentId, long ExpectedVersion) : ICommand<VersionedResponse<SpecimenId>>;
