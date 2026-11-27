using Kmic.BuildingBlocks.Application.Execution;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

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

    // 导出用流式连接池
    public NpgsqlDataSource Stream =>
        _workerStream ?? throw new InvalidOperationException("没有配置流式池（Kmic:Database:Worker:StreamMaxPoolSize）。");

    // 仅 HTTP 请求使用前台连接池；Contracts 模块间调用沿用初始入口来源（不会以 ModuleCall 作为根入口）
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

        // 禁用自动预编译：数据权限条件需根据实际参数值生成执行计划以命中索引，通用预编译计划可能退化为全表扫描
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
