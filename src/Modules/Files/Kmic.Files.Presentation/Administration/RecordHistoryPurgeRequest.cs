namespace Kmic.Files.Presentation.Administration;

/// <summary>
/// 登记彻底删除历史版本的请求
/// </summary>
/// <param name="Note">执行记录，如工单号，必填</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record RecordHistoryPurgeRequest(string Note, long ExpectedVersion);
