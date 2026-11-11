namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 一个模块的 Outbox、Inbox 表名，带引号，可以直接拼进 SQL
/// </summary>
/// <param name="Outbox">Outbox 表</param>
/// <param name="Inbox">Inbox 表</param>
internal sealed record MessageTables(string Outbox, string Inbox);
