using Kmic.BuildingBlocks.Application.Execution;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 连接池
/// </summary>
/// <remarks>
/// 写入上下文只用事务池，查询上下文只用读池。命令占着事务连接时，嵌套查询从读池借，不会等事务池，
/// 所以并发命令把事务池占满也不会互相卡死。创建时不连接数据库
/// </remarks>
internal sealed class DataSources : IAsyncDisposable
{
    private readonly NpgsqlDataSource _apiTransactions;
    private readonly NpgsqlDataSource _apiReads;
    private readonly NpgsqlDataSource _workerTransactions;
    private readonly NpgsqlDataSource _workerReads;
    private readonly NpgsqlDataSource? _workerStream;

    public DataSources(IOptions<DatabaseOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var database = options.Value;
        var connection = Parse(database.ConnectionString);

        _apiTransactions = Create(connection, "kmic-api-tx", database.Api.TxMaxPoolSize);
        _apiReads = Create(connection, "kmic-api-read", database.Api.ReadMaxPoolSize);
        _workerTransactions = Create(connection, "kmic-worker-tx", database.Worker.TxMaxPoolSize);
        _workerReads = Create(connection, "kmic-worker-read", database.Worker.ReadMaxPoolSize);
        if (database.Worker.StreamMaxPoolSize > 0)
        {
            _workerStream = Create(connection, "kmic-worker-stream", database.Worker.StreamMaxPoolSize);
        }
    }

    /// <summary>
    /// 流式池，导出用
    /// </summary>
    public NpgsqlDataSource Stream =>
        _workerStream ?? throw new InvalidOperationException("没有配置流式池（Kmic:Database:Worker:StreamMaxPoolSize）。");

    // 只有 HTTP 请求用前台池。Contracts 调用沿用入口的来源，入口不会是 ModuleCall
    public NpgsqlDataSource Transactions(CallOrigin entry) => entry == CallOrigin.Http ? _apiTransactions : _workerTransactions;

    public NpgsqlDataSource Reads(CallOrigin entry) => entry == CallOrigin.Http ? _apiReads : _workerReads;

    public async ValueTask DisposeAsync()
    {
        await _apiTransactions.DisposeAsync();
        await _apiReads.DisposeAsync();
        await _workerTransactions.DisposeAsync();
        await _workerReads.DisposeAsync();
        if (_workerStream is not null)
        {
            await _workerStream.DisposeAsync();
        }
    }

    private static NpgsqlConnectionStringBuilder Parse(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("没有配置数据库连接串（ConnectionStrings:Kmic）。");
        }

        var connection = new NpgsqlConnectionStringBuilder(connectionString);

        // 关闭自动预编译：数据权限条件要按实际参数值做执行计划才用得上索引，预编译的通用计划会退化成全表扫描
        return connection.MaxAutoPrepare == 0
            ? connection
            : throw new InvalidOperationException("连接串里的 Max Auto Prepare 必须为 0。");
    }

    private static NpgsqlDataSource Create(NpgsqlConnectionStringBuilder connection, string applicationName, int maxPoolSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxPoolSize, 1);
        var builder = new NpgsqlDataSourceBuilder(connection.ConnectionString);
        builder.ConnectionStringBuilder.ApplicationName = applicationName;
        builder.ConnectionStringBuilder.MaxPoolSize = maxPoolSize;
        return builder.Build();
    }
}
