namespace Kmic.Sample.Presentation.Specimens;

/// <summary>
/// 移除示例附件的请求
/// </summary>
/// <param name="ExpectedVersion">读取时的版本号，与当前版本不一致时返回 409 Common.VersionConflict</param>
public sealed record RemoveSpecimenAttachmentRequest(long ExpectedVersion);
