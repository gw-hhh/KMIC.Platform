using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kmic.MigrationRunner;

internal sealed class RunnerContext(IConfiguration configuration, IReadOnlyList<IModule> modules, TextWriter output, TimeProvider time)
{
    private readonly Lazy<NumberRuleCatalog> _rules = new(() => NumberRuleCatalog.FromAssemblies(modules.Select(m => m.ApplicationAssembly)));

    public IReadOnlyList<IModule> Modules => modules;

    public TextWriter Output => output;

    public TimeProvider Time => time;

    public NumberRuleCatalog Rules => _rules.Value;

    public string ConnectionString() =>
        configuration[RunnerApp.ConnectionStringKey] is { Length: > 0 } connection
            ? KmicConnectionString.Normalize(connection)
            : throw new InvalidOperationException($"未配置迁移账号连接字符串 {RunnerApp.ConnectionStringKey}。");

    // 仅注册迁移所需的模块数据库上下文，不启动 Web 宿主或后台任务
    public ServiceProvider Services()
    {
        var services = new ServiceCollection();
        foreach (var module in modules)
        {
            module.AddServices(services);
        }

        return services.BuildServiceProvider();
    }

    // 只输出错误原因并返回 1，避免将异常堆栈写入部署日志
    public async Task<int> GuardedAsync(Func<Task<int>> run)
    {
        try
        {
            return await run();
        }
        catch (Exception exception) when (exception is InvalidOperationException or NpgsqlException or ArgumentException)
        {
            await output.WriteLineAsync($"失败：{exception.Message}");
            return 1;
        }
    }
}
