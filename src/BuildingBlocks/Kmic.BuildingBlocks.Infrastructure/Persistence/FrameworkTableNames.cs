namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

// 单个模块的框架表名（已加引号，可直接拼接至 SQL）
internal sealed record FrameworkTableNames(string Idempotency, string Outbox, string Inbox, string Jobs, string RebuildRuns, string NumberCounters);
