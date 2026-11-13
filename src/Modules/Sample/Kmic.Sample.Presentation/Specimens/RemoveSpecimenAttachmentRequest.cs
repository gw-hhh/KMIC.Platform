namespace Kmic.Sample.Presentation.Specimens;

/// <summary>
/// 移除标本附件的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record RemoveSpecimenAttachmentRequest(long ExpectedVersion);
