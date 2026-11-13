namespace Kmic.Files.Presentation.Administration;

/// <summary>
/// 彻底删除文件的请求
/// </summary>
/// <param name="Reason">原因，必填</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record PurgeFileRequest(string Reason, long ExpectedVersion);
