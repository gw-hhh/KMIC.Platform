using System.CommandLine;
using Kmic.BuildingBlocks.Infrastructure.Maintenance;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Kmic.MigrationRunner;

// 退出码：0 成功，1 失败，2 参数错误，3 存在待执行迁移（仅 status）
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
        var root = new RootCommand("Kmic 数据库迁移工具：使用迁移账号执行模块迁移及数据库维护操作")
        {
            MigrateCommand(context),
            StatusCommand(context),
            PurgeCommand(context),
            NumbersCommands.Build(context),
        };

        // 命令行参数解析错误统一返回退出码 2，以区分执行失败（1）
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
        var command = new Command("migrate", "执行所有模块的待处理迁移。先完成数据库级初始化（如不区分大小写排序规则），再按模块名称依次执行");
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
        var command = new Command("status", "列出待执行的迁移（只读检查）；存在待执行迁移时返回退出码 3，供部署流程判断");
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
            Description = "待清理的目标表",
            Required = true,
        };
        table.AcceptOnlyFromAmong([.. AppendOnlyPurge.Targets.Keys]);
        var days = new Option<int>("--older-than-days")
        {
            Description = "数据保留天数，清理早于该截止时间的记录；须按运维规范显式指定，无默认值",
            Required = true,
        };
        var batch = new Option<int>("--batch-size")
        {
            Description = "每批删除的行数",
            DefaultValueFactory = _ => DefaultBatchSize,
        };

        var command = new Command("purge", "分批清理只追加表（审计日志、安全事件）中超出保留期的记录；应用运行账号无删除权限，须由迁移账号执行")
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
                await context.Output.WriteLineAsync("保留期和每批行数均须大于 0。");
                return UsageExitCode;
            }

            var before = context.Time.GetUtcNow().AddDays(-retention);
            await using var database = NpgsqlDataSource.Create(context.ConnectionString());
            var name = parse.GetValue(table)!;
            var deleted = await AppendOnlyPurge.RunAsync(database, AppendOnlyPurge.Targets[name], before, size, ct);
            await context.Output.WriteLineAsync(deleted is null
                ? $"{name} 对应的表不存在，没有可清理的数据。"
                : $"{name}：删除了 {deleted} 行（{before:yyyy-MM-dd HH:mm} UTC 之前）。");
            return 0;
        }));
        return command;
    }
}
