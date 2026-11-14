namespace Kmic.Sample.Presentation.Specimens;

/// <summary>
/// 添加示例附件的请求
/// </summary>
/// <param name="FileId">上传得到的文件 ID</param>
/// <param name="ExpectedVersion">读取时的版本号，与当前版本不一致时返回 409 Common.VersionConflict</param>
public sealed record AddSpecimenAttachmentRequest(Guid FileId, long ExpectedVersion);
