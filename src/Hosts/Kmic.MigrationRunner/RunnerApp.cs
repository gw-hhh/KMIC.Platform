using System.CommandLine;
using Kmic.BuildingBlocks.Infrastructure.Maintenance;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Kmic.MigrationRunner;

/// <summary>
/// 迁移程序的命令
/// </summary>
/// <remarks>
/// 退出码：0 成功；1 执行失败；2 命令行写错了；3 status 发现有未执行的迁移。
/// 连接串只从配置读（环境变量 Kmic__Migration__ConnectionString 或密钥管理注入），不接受命令行参数，免得出现在进程列表和日志里
/// </remarks>
internal static class RunnerApp
{
    public const string ConnectionStringKey = "Kmic:Migration:ConnectionString";

    public const int PendingExitCode = 3;

    public const int UsageExitCode = 2;

    private const int DefaultBatchSize = 5000;

    public static async Task<int> RunAsync(
        string[] args,
        IConfiguration configuration,
        ToolModules modules,
        bool production,
        TextWriter output,
        TimeProvider time,
        CancellationToken ct)
    {
        var context = new RunnerContext(configuration, modules.For(production), output, time);
        var root = new RootCommand("Kmic 数据库迁移程序：用迁移账号执行各模块的迁移，并做数据库层面的维护")
        {
            MigrateCommand(context),
            StatusCommand(context),
            PurgeCommand(context),
            NumbersCommands.Build(context),
        };

        // 命令行写错了统一返回 2：System.CommandLine 默认返回 1，会和“执行失败”混在一起
        var parsed = root.Parse(args);
        if (parsed.Errors.Count > 0)
        {
            foreach (var error in parsed.Errors)
            {
                await output.WriteLineAsync(error.Message);
            }

            return UsageExitCode;
        }

        return await parsed.InvokeAsync(new InvocationConfiguration { Output = output, Error = output }, ct);
    }

    private static Command MigrateCommand(RunnerContext context)
    {
        var command = new Command("migrate", "执行全部模块未执行的迁移。先做数据库级的准备（不区分大小写的排序规则），再按模块名称顺序逐个迁移");
        command.SetAction((_, ct) => context.GuardedAsync(async () =>
        {
            var connection = context.ConnectionString();
            await using var services = context.Services();
            var pending = await ModuleMigrator.PendingAsync(services, connection, ct);
            await context.Output.WriteLineAsync($"涉及 {context.Modules.Count} 个模块：{string.Join("、", context.Modules.Select(m => m.Name))}");
            await context.Output.WriteLineAsync(pending.Count == 0 ? "没有未执行的迁移。" : $"待执行 {pending.Count} 个迁移：{string.Join("、", pending)}");

            await ModuleMigrator.MigrateAllAsync(services, connection, ct);
            await context.Output.WriteLineAsync("迁移完成。");
            return 0;
        }));
        return command;
    }

    private static Command StatusCommand(RunnerContext context)
    {
        var command = new Command("status", "列出未执行的迁移，不改库。有未执行的迁移时退出码为 3，部署流程可以用它判断");
        command.SetAction((_, ct) => context.GuardedAsync(async () =>
        {
            await using var services = context.Services();
            var pending = await ModuleMigrator.PendingAsync(services, context.ConnectionString(), ct);
            if (pending.Count == 0)
            {
                await context.Output.WriteLineAsync("没有未执行的迁移。");
                return 0;
            }

            foreach (var name in pending)
            {
                await context.Output.WriteLineAsync(name);
            }

            return PendingExitCode;
        }));
        return command;
    }

    private static Command PurgeCommand(RunnerContext context)
    {
        var table = new Option<string>("--table")
        {
            Description = "要清理的表",
            Required = true,
        };
        table.AcceptOnlyFromAmong([.. AppendOnlyPurge.Targets.Keys]);
        var days = new Option<int>("--older-than-days")
        {
            Description = "保留期（天）：发生时间早于这么多天之前的行被删除。保留期由运维按制度给出，没有默认值",
            Required = true,
        };
        var batch = new Option<int>("--batch-size")
        {
            Description = "每批删除的行数",
            DefaultValueFactory = _ => DefaultBatchSize,
        };

        var command = new Command("purge", "分批删除只追加表（审计日志、安全事件）里超过保留期的行；运行账号对这些表没有删除权限，只能由迁移账号执行")
        {
            table,
            days,
            batch,
        };
        command.SetAction((parse, ct) => context.GuardedAsync(async () =>
        {
            var retention = parse.GetValue(days);
            var size = parse.GetValue(batch);
            if (retention < 1 || size < 1)
            {
                await context.Output.WriteLineAsync("保留期和每批行数都要大于 0。");
                return UsageExitCode;
            }

            var before = context.Time.GetUtcNow().AddDays(-retention);
            await using var database = NpgsqlDataSource.Create(context.ConnectionString());
            var name = parse.GetValue(table)!;
            var deleted = await AppendOnlyPurge.RunAsync(database, AppendOnlyPurge.Targets[name], before, size, ct);
            await context.Output.WriteLineAsync(deleted is null
                ? $"{name} 对应的表还不存在（模块尚未建立），没有可清理的数据。"
                : $"{name}：删除了 {deleted} 行（{before:yyyy-MM-dd HH:mm} UTC 之前）。");
            return 0;
        }));
        return command;
    }
}
