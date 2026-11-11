namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 一个模块的框架表名，带引号，可以直接拼进 SQL
/// </summary>
/// <param name="Idempotency">幂等记录表</param>
/// <param name="Outbox">Outbox 表</param>
/// <param name="Inbox">Inbox 表</param>
/// <param name="Jobs">后台作业表</param>
internal sealed record FrameworkTableNames(string Idempotency, string Outbox, string Inbox, string Jobs);
