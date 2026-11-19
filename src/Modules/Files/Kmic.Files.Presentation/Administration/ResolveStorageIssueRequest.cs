namespace Kmic.Files.Presentation.Administration;

/// <summary>
/// 标记存储问题已解决的请求
/// </summary>
/// <param name="Note">处理说明</param>
/// <param name="ExpectedVersion">预期版本号</param>
public sealed record ResolveStorageIssueRequest(string Note, long ExpectedVersion);
