using System.CommandLine;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Infrastructure.Operations;
using Kmic.Operations.Application.MessagingCapacity;

namespace Kmic.Operator;

/// <summary>
/// Operations 模块的运维命令
/// </summary>
internal static class OperationsCommands
{
    public static Command Build(OperatorContext context) =>
        new("operations", "Operations 模块的运维命令")
        {
            SetMessagingCapacity(context),
        };

    private static Command SetMessagingCapacity(OperatorContext context)
    {
        var bytes = new Option<long>("--bytes") { Description = "数据库当前已分配的存储容量（字节）；0 表示不判断", Required = true };
        var command = new Command(
            "set-messaging-capacity",
            "写入数据库已分配的存储容量，消息写入的容量保护据此判断空间是否紧张。部署执行机读取 RDS 容量后定期调用，超过一天没更新会告警")
        {
            bytes,
        };
        command.SetAction((parse, ct) => context.GuardedAsync(async () =>
        {
            var definition = SystemSettingCatalog.Build([typeof(MessagingCapacitySetting)]).Get<MessagingCapacitySetting>();
            var result = await ConfigurationOperator.SetSettingAsync(
                context.ConnectionString(), definition, new MessagingCapacitySetting(parse.GetRequiredValue(bytes)), "运维：更新数据库已分配容量", ct);
            if (result.IsSuccess)
            {
                await context.Output.WriteLineAsync("已写入。");
                return 0;
            }

            // 值没通过校验是参数问题，其余是执行失败
            var reasons = result.Error.FieldErrors.Count > 0 ? string.Join("；", result.Error.FieldErrors.Select(f => f.Message)) : result.Error.Message;
            await context.Output.WriteLineAsync($"失败：{reasons}");
            return result.Error.Type == ErrorType.Validation ? OperatorApp.UsageExitCode : 1;
        }));
        return command;
    }
}
