namespace Kmic.Files.Presentation.Administration;

/// <param name="Note">执行记录</param>
/// <param name="ExpectedVersion">预期版本号</param>
public sealed record RecordHistoryPurgeRequest(string Note, long ExpectedVersion);
