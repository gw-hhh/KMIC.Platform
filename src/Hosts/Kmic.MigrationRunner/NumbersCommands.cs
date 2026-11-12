using System.CommandLine;
using System.Globalization;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Numbering;
using Npgsql;

namespace Kmic.MigrationRunner;

/// <summary>
/// 业务编号计数的维护命令
/// </summary>
/// <remarks>
/// 导入旧数据之后，把计数抬到旧编号的最大序号，新编号才不会和旧编号重复
/// </remarks>
internal static class NumbersCommands
{
    private const string DateFormat = "yyyy-MM-dd";

    public static Command Build(RunnerContext context) =>
        new("numbers", "业务编号的计数")
        {
            RulesCommand(context),
            EnsureAtLeastCommand(context),
        };

    private static Command RulesCommand(RunnerContext context)
    {
        var command = new Command("rules", "列出本次处理的模块里声明的全部编号规则");
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
        var rule = new Option<string>("--rule") { Description = "规则名称，rules 命令可以列出", Required = true };
        var value = new Option<long>("--value") { Description = "旧数据里这个周期与分组下的最大序号", Required = true };
        var date = new Option<string>("--date") { Description = $"旧编号所在的日期（{DateFormat}），决定属于哪个周期；周期重置的规则必填" };
        var partition = new Option<string>("--partition") { Description = "分组键，按分组的规则必填" };

        var command = new Command("ensure-at-least", "保证计数不小于给定值：新编号从这个值加一开始；已经更大的计数不会回退，可以重复执行")
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
                await context.Output.WriteLineAsync($"没有编号规则 {name}。用 numbers rules 查看全部规则。");
                return RunnerApp.UsageExitCode;
            }

            var day = parse.GetValue(date);
            if (day is null && definition.Reset != NumberReset.Never)
            {
                await context.Output.WriteLineAsync($"规则 {name} 按{definition.Reset}重置，要用 --date 指明旧编号所在的日期。");
                return RunnerApp.UsageExitCode;
            }

            // 不重置的规则周期键固定，日期随便取
            if (!DateOnly.TryParseExact(day ?? "2000-01-01", DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var businessDate))
            {
                await context.Output.WriteLineAsync($"--date 的格式是 {DateFormat}。");
                return RunnerApp.UsageExitCode;
            }

            var group = parse.GetValue(partition);
            await using var database = NpgsqlDataSource.Create(context.ConnectionString());
            var current = await NumberCounters.EnsureAtLeastAsync(
                database, definition.Module, definition, businessDate, group, parse.GetValue(value), context.Time, ct);
            await context.Output.WriteLineAsync(
                $"{name}（周期 {definition.PeriodKey(businessDate)}，分组 {definition.PartitionKey(group)}）的计数现在是 {current}，下一个序号是 {current + 1}。");
            return 0;
        }));
        return command;
    }
}
