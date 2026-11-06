using Npgsql;
using Xunit;

namespace Spike06.Tests;

// 类级夹具：每个测试类从模板库克隆一个数据库；类内的测试串行共用它
public sealed class ClassDatabase(TemplateDatabase template) : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = null!;

    public NpgsqlDataSource Source { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        ConnectionString = await template.CloneAsync();
        Source = NpgsqlDataSource.Create(ConnectionString);
    }

    public async ValueTask DisposeAsync() => await Source.DisposeAsync();
}
