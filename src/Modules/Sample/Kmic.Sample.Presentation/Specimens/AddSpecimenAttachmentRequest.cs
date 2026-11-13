namespace Kmic.Sample.Presentation.Specimens;

/// <summary>
/// 添加标本附件的请求
/// </summary>
/// <param name="FileId">上传得到的文件 ID</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record AddSpecimenAttachmentRequest(Guid FileId, long ExpectedVersion);
