using System.CommandLine;
using System.Globalization;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Numbering;
using Npgsql;

namespace Kmic.MigrationRunner;

internal static class NumbersCommands
{
    private const string DateFormat = "yyyy-MM-dd";

    public static Command Build(RunnerContext context) =>
        new("numbers", "业务编号计数维护")
        {
            RulesCommand(context),
            EnsureAtLeastCommand(context),
        };

    private static Command RulesCommand(RunnerContext context)
    {
        var command = new Command("rules", "列出当前加载模块中声明的全部编号规则");
        command.SetAction(async _ =>
        {
            foreach (var rule in context.Rules.All.OrderBy(r => r.Name, StringComparer.Ordinal))
            {
                var partition = rule.Partitioned ? "按分组" : "不分组";
                await context.Output.WriteLineAsync($"{rule.Name}\t{rule.Format}\t周期：{rule.Reset}\t{partition}");
            }
        });
        return command;
    }

    private static Command EnsureAtLeastCommand(RunnerContext context)
    {
        var rule = new Option<string>("--rule") { Description = "规则名称（可通过 rules 子命令查看）", Required = true };
        var value = new Option<long>("--value") { Description = "历史数据在指定周期与分组下的最大序号", Required = true };
        var date = new Option<string>("--date") { Description = $"历史编号所属日期（{DateFormat}），用于确定计数周期；按周期重置的规则必填" };
        var partition = new Option<string>("--partition") { Description = "分组键，按分组的规则必填" };

        var command = new Command("ensure-at-least", "将计数提升至不低于指定值（不回退已有计数）；下一序号为调整后计数加 1，操作幂等")
        {
            rule,
            value,
            date,
            partition,
        };
        command.SetAction((parse, ct) => context.GuardedAsync(async () =>
        {
            var name = parse.GetValue(rule)!;
            var definition = context.Rules.All.FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.Ordinal));
            if (definition is null)
            {
                await context.Output.WriteLineAsync($"未找到编号规则 {name}，请使用 numbers rules 查看全部规则。");
                return RunnerApp.UsageExitCode;
            }

            var day = parse.GetValue(date);
            if (day is null && definition.Reset != NumberReset.Never)
            {
                await context.Output.WriteLineAsync($"规则 {name} 的重置周期为 {definition.Reset}，请通过 --date 指定业务日期。");
                return RunnerApp.UsageExitCode;
            }

            // 不重置的规则使用固定周期键，日期不影响计数范围
            if (!DateOnly.TryParseExact(day ?? "2000-01-01", DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var businessDate))
            {
                await context.Output.WriteLineAsync($"--date 格式须为 {DateFormat}。");
                return RunnerApp.UsageExitCode;
            }

            var group = parse.GetValue(partition);
            await using var database = NpgsqlDataSource.Create(context.ConnectionString());
            var current = await NumberCounters.EnsureAtLeastAsync(
                database, definition.Module, definition, businessDate, group, parse.GetValue(value), context.Time, ct);
            await context.Output.WriteLineAsync(
                $"{name}（周期 {definition.PeriodKey(businessDate)}，分组 {definition.PartitionKey(group)}）当前计数为 {current}，下一个序号为 {current + 1}。");
            return 0;
        }));
        return command;
    }
}
