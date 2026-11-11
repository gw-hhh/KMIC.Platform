using Kmic.BuildingBlocks.Testing.Database;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 本测试程序集的模板库：按 Probe 模型建表（迁移实现后改为执行迁移）
/// </summary>
public sealed class ProbeTemplate : PostgresTemplateDatabase
{
    protected override async Task InitializeTemplateAsync(string connectionString, CancellationToken cancellationToken)
    {
        await using var harness = new ProbeHarness(connectionString);
        await using var work = harness.BeginAsSystem();
        await work.Db.Database.EnsureCreatedAsync(cancellationToken);
    }
}
