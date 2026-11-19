namespace Kmic.Files.Presentation.Administration;

/// <summary>
/// 彻底删除文件的请求
/// </summary>
/// <param name="Reason">原因</param>
/// <param name="ExpectedVersion">预期版本号</param>
public sealed record PurgeFileRequest(string Reason, long ExpectedVersion);
