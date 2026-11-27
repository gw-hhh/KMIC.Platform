namespace Kmic.Sample.Presentation.Specimens;

/// <param name="FileId">上传得到的文件 ID</param>
/// <param name="ExpectedVersion">读取时的版本号</param>
public sealed record AddSpecimenAttachmentRequest(Guid FileId, long ExpectedVersion);
