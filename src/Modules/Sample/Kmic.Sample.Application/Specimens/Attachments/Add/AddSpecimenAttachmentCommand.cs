using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.Add;

/// <summary>为示例添加附件（关联前须先上传文件）</summary>
/// <param name="Id">示例 ID</param>
/// <param name="FileId">上传得到的文件 ID</param>
/// <param name="ExpectedVersion">读取时的版本号</param>
[RequirePermission(SamplePermissions.Edit)]
[Audited]
public sealed record AddSpecimenAttachmentCommand(SpecimenId Id, StoredFileId FileId, long ExpectedVersion) : ICommand<VersionedResponse<SpecimenId>>;
