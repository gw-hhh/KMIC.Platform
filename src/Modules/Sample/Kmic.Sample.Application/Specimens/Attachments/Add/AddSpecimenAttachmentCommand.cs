using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.Add;

/// <summary>
/// 为示例添加附件
/// </summary>
/// <remarks>
/// 文件须先上传，调用时传入上传返回的文件 ID。
/// 通过 IFileAttachments 获取文件锁、检查文件并发出关联请求，再保存附件信息，全部在同一个事务中完成
/// </remarks>
/// <param name="Id">示例 ID</param>
/// <param name="FileId">上传得到的文件 ID</param>
/// <param name="ExpectedVersion">读取时的版本号，与当前版本不一致时返回版本冲突</param>
[RequirePermission(SamplePermissions.Edit)]
[Audited]
public sealed record AddSpecimenAttachmentCommand(SpecimenId Id, StoredFileId FileId, long ExpectedVersion) : ICommand<VersionedResponse<SpecimenId>>;
