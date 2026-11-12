namespace Kmic.Sample.Presentation.Specimens;

/// <summary>
/// 删除示例标本的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record DeleteSpecimenRequest(long ExpectedVersion);
