using System.Diagnostics;
using Npgsql;
using Shouldly;
using Xunit;

namespace Spike06.Tests;

// 会在模板库上留下连接的测试单独串行运行，不影响其他测试类的克隆
[Collection(nameof(TemplateExclusive))]
public sealed class TemplateCloneTests(TemplateDatabase template)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Clone_fails_while_a_pooled_connection_to_the_template_stays_open()
    {
        // 连接关闭后回到连接池，物理连接仍然连着模板库
        var pooled = NpgsqlDataSource.Create(template.ConnectionString(TemplateDatabase.TemplateName, pooling: true));
        await using (var connection = await pooled.OpenConnectionAsync(Ct))
        {
            await Db.ExecAsync(connection, null, "SELECT 1");
        }

        var error = await Should.ThrowAsync<PostgresException>(() => template.CloneAsync());
        Log(error.MessageText);
        error.SqlState.ShouldBe(PostgresErrorCodes.ObjectInUse);

        // 释放数据源（关闭池中的物理连接）之后即可克隆
        await pooled.DisposeAsync();
        await template.CloneAsync();
    }

    [Fact]
    public async Task Concurrent_clones_from_the_same_template()
    {
        var started = Stopwatch.GetTimestamp();

        var clones = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => template.CloneAsync()));

        clones.Distinct(StringComparer.Ordinal).Count().ShouldBe(8);
        Log($"并发克隆 8 个库：{Stopwatch.GetElapsedTime(started).TotalMilliseconds:F0} ms；模板库迁移：{template.MigrationTime.TotalMilliseconds:F0} ms");
    }

    [Theory]
    [InlineData("WAL_LOG")]
    [InlineData("FILE_COPY")]
    public async Task Clone_time_by_strategy(string strategy)
    {
        var times = new List<double>();
        for (var i = 0; i < 5; i++)
        {
            var started = Stopwatch.GetTimestamp();
            await template.CloneAsync(strategy);
            times.Add(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }

        Log($"{strategy}：{string.Join(", ", times.Select(t => t.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)))} ms");
    }

    private static void Log(string text) => TestContext.Current.TestOutputHelper?.WriteLine(text);
}
