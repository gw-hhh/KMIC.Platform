using Npgsql;
using Xunit;

namespace Kmic.BuildingBlocks.Testing.Database;

/// <summary>
/// 测试类的数据库
/// </summary>
/// <remarks>
/// 每个测试类一个，从模板库克隆，类结束时删除。同一个类里的测试共用它和 <see cref="Shared{T}"/> 里的对象，各自只断言自己的数据；需要空库的测试放进单独的类
/// </remarks>
/// <typeparam name="TTemplate">模板库</typeparam>
public sealed class ClassDatabase<TTemplate>(TTemplate template) : IAsyncLifetime
    where TTemplate : PostgresTemplateDatabase
{
    private readonly Lock _gate = new();
    private readonly List<IAsyncDisposable> _shared = [];
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
    /// 数据源，用来直接执行 SQL
    /// </summary>
    public NpgsqlDataSource DataSource => _dataSource ?? throw NotInitialized();

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        _name = await template.CloneAsync(TestContext.Current.CancellationToken);
        _dataSource = NpgsqlDataSource.Create(template.ConnectionString(_name));
    }

    /// <summary>
    /// 取同一个测试类共用的对象（如测试宿主），第一次取时创建
    /// </summary>
    /// <remarks>
    /// 同一类型只创建一次，之后传入的 create 不再调用，所以同一个类里它用到的参数要一致
    /// </remarks>
    public T Shared<T>(Func<T> create)
        where T : class, IAsyncDisposable
    {
        ArgumentNullException.ThrowIfNull(create);
        lock (_gate)
        {
            if (_shared.OfType<T>().FirstOrDefault() is { } existing)
            {
                return existing;
            }

            var created = create();
            _shared.Add(created);
            return created;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        // 先释放共用对象再删库：宿主的后台循环还连着这个库，确保它们先停下
        for (var i = _shared.Count - 1; i >= 0; i--)
        {
            await _shared[i].DisposeAsync();
        }

        if (_dataSource is not null)
        {
            await _dataSource.DisposeAsync();
        }

        // 测试已结束，取消令牌可能已经触发，但删除必须做完，不然本地容器里的克隆库会越积越多
        if (_name is not null)
        {
            await template.DropAsync(_name, CancellationToken.None);
        }
    }

    private static InvalidOperationException NotInitialized() => new("克隆库尚未建立：ClassDatabase 只能作为类级夹具使用。");
}
