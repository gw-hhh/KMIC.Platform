namespace Kmic.Operations.Presentation.Messages;

/// <summary>
/// 触发、确认、放弃读模型重建的请求
/// </summary>
/// <param name="Subscription">订阅名</param>
public sealed record RebuildRequest(string Subscription);
