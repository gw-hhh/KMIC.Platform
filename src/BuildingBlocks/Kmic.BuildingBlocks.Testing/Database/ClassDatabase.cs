using Npgsql;
using Xunit;

namespace Kmic.BuildingBlocks.Testing.Database;

/// <summary>测试类级隔离数据库（从模板库克隆，类结束时自动删除）；同一测试类共用该库，各用例应仅断言自身写入的数据。</summary>
/// <typeparam name="TTemplate">模板数据库类型</typeparam>
public sealed class ClassDatabase<TTemplate>(TTemplate template) : IAsyncLifetime
    where TTemplate : PostgresTemplateDatabase
{
    private readonly Lock _gate = new();
    private readonly List<IAsyncDisposable> _shared = [];
    private string? _name;
    private NpgsqlDataSource? _dataSource;

    public string Name => _name ?? throw NotInitialized();

    public string ConnectionString => template.ConnectionString(Name);

    /// <summary>数据源实例，用于直接执行 SQL。</summary>
    public NpgsqlDataSource DataSource => _dataSource ?? throw NotInitialized();

    public async ValueTask InitializeAsync()
    {
        _name = await template.CloneAsync(TestContext.Current.CancellationToken);
        _dataSource = NpgsqlDataSource.Create(template.ConnectionString(_name));
    }

    /// <summary>获取或创建测试类共享对象（同一类型仅创建一次，后续调用不再执行 create）。</summary>
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

    public async ValueTask DisposeAsync()
    {
        // 先释放共享对象再删库：宿主后台循环仍持有该库连接，需确保其先停止
        for (var i = _shared.Count - 1; i >= 0; i--)
        {
            await _shared[i].DisposeAsync();
        }

        if (_dataSource is not null)
        {
            await _dataSource.DisposeAsync();
        }

        // 忽略取消令牌以强制清理克隆库，避免本地容器产生残留
        if (_name is not null)
        {
            await template.DropAsync(_name, CancellationToken.None);
        }
    }

    private static InvalidOperationException NotInitialized() => new("克隆库尚未建立：ClassDatabase 只能作为类级夹具使用。");
}
