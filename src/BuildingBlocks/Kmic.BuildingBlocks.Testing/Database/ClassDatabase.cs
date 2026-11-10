using Npgsql;
using Xunit;

namespace Kmic.BuildingBlocks.Testing.Database;

/// <summary>
/// 类级夹具：每个测试类一个从模板克隆的数据库，类结束时删除
/// </summary>
/// <remarks>
/// 同一个类中的测试共用这个库，各自创建并只断言自己的数据；需要空库语义的测试放进单独的类
/// </remarks>
/// <typeparam name="TTemplate">本测试程序集注册的模板库夹具</typeparam>
public sealed class ClassDatabase<TTemplate>(TTemplate template) : IAsyncLifetime
    where TTemplate : PostgresTemplateDatabase
{
    private string? _name;
    private NpgsqlDataSource? _dataSource;

    /// <summary>
    /// 库名
    /// </summary>
    public string Name => _name ?? throw NotInitialized();

    /// <summary>
    /// 连接串
    /// </summary>
    public string ConnectionString => template.ConnectionString(Name);

    /// <summary>
    /// 直接执行 SQL 用的数据源，如准备数据、检查结果
    /// </summary>
    public NpgsqlDataSource DataSource => _dataSource ?? throw NotInitialized();

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        _name = await template.CloneAsync(TestContext.Current.CancellationToken);
        _dataSource = NpgsqlDataSource.Create(template.ConnectionString(_name));
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_dataSource is not null)
        {
            await _dataSource.DisposeAsync();
        }

        // 测试已经结束，取消令牌可能已触发；删除必须做完，否则长时间运行的本地容器会积累克隆库
        if (_name is not null)
        {
            await template.DropAsync(_name, CancellationToken.None);
        }
    }

    private static InvalidOperationException NotInitialized() => new("克隆库尚未建立：ClassDatabase 只能作为类级夹具使用。");
}
