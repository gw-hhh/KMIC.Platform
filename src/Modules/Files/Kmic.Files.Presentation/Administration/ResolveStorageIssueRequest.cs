namespace Kmic.Files.Presentation.Administration;

/// <summary>
/// 标记存储问题已解决的请求
/// </summary>
/// <param name="Note">处理说明，必填，如还原的工单号</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record ResolveStorageIssueRequest(string Note, long ExpectedVersion);
