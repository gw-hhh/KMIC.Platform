namespace Kmic.Notifications.Application.Templates.Sync;

/// <summary>
/// 模板同步的结果
/// </summary>
/// <param name="Added">新登记的模板数</param>
/// <param name="Updated">更新或恢复声明的模板数</param>
/// <param name="Retired">新退役的模板数</param>
public sealed record TemplateSyncResult(int Added, int Updated, int Retired);
