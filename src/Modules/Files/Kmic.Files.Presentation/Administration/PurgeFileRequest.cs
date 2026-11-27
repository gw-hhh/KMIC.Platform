namespace Kmic.Files.Presentation.Administration;

/// <param name="Reason">原因</param>
/// <param name="ExpectedVersion">预期版本号</param>
public sealed record PurgeFileRequest(string Reason, long ExpectedVersion);
