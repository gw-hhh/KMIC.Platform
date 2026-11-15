namespace Kmic.Files.Presentation.Administration;

/// <summary>
/// 登记文件历史版本清除记录的请求
/// </summary>
/// <param name="Note">执行记录，如工单号，必填</param>
/// <param name="ExpectedVersion">预期版本号，与当前版本不符时返回 409 Common.VersionConflict</param>
public sealed record RecordHistoryPurgeRequest(string Note, long ExpectedVersion);
