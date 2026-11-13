using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.Add;

/// <summary>
/// 给示例标本添加一个附件：文件要先经上传端点上传，凭得到的文件 ID 关联
/// </summary>
/// <remarks>
/// 关联走框架的 IFileAttachments：先取文件的共享锁、检查文件现在能不能关联，再写本模块的附件行并发出关联请求，三步在同一个事务里
/// </remarks>
/// <param name="Id">标本</param>
/// <param name="FileId">上传得到的文件 ID</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
[RequirePermission(SamplePermissions.Edit)]
[Audited]
public sealed record AddSpecimenAttachmentCommand(SpecimenId Id, StoredFileId FileId, long ExpectedVersion) : ICommand<VersionedResponse<SpecimenId>>;
