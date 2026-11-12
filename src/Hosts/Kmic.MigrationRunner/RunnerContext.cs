using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kmic.MigrationRunner;

/// <summary>
/// 各个命令共用的环境：配置、本次要处理的模块、输出和时间
/// </summary>
internal sealed class RunnerContext(IConfiguration configuration, IReadOnlyList<IModule> modules, TextWriter output, TimeProvider time)
{
    private readonly Lazy<NumberRuleCatalog> _rules = new(() => NumberRuleCatalog.FromAssemblies(modules.Select(m => m.ApplicationAssembly)));

    public IReadOnlyList<IModule> Modules => modules;

    public TextWriter Output => output;

    public TimeProvider Time => time;

    public NumberRuleCatalog Rules => _rules.Value;

    public string ConnectionString() =>
        configuration[RunnerApp.ConnectionStringKey] is { Length: > 0 } connection
            ? connection
            : throw new InvalidOperationException($"没有配置迁移账号的连接串 {RunnerApp.ConnectionStringKey}。");

    // 模块的服务注册类只登记自己的数据库上下文，迁移只需要这些登记
    public ServiceProvider Services()
    {
        var services = new ServiceCollection();
        foreach (var module in modules)
        {
            module.AddServices(services);
        }

        return services.BuildServiceProvider();
    }

    // 参数不合规（分组键格式、序号不是正数）和连不上数据库都说明原因并返回 1，不让异常堆栈直接打到部署日志里
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
