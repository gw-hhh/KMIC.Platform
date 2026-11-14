namespace Kmic.Files.Presentation.Administration;

/// <summary>
/// 彻底删除文件的请求
/// </summary>
/// <param name="Reason">原因，必填</param>
/// <param name="ExpectedVersion">预期版本号，与当前版本不符时返回 409 Common.VersionConflict</param>
public sealed record PurgeFileRequest(string Reason, long ExpectedVersion);
