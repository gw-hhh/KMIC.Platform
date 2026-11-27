using System.CommandLine;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Infrastructure.Operations;
using Kmic.Operations.Application.MessagingCapacity;

namespace Kmic.Operator;

internal static class OperationsCommands
{
    public static Command Build(OperatorContext context) =>
        new("operations", "Operations 模块的运维命令")
        {
            SetMessagingCapacity(context),
        };

    private static Command SetMessagingCapacity(OperatorContext context)
    {
        var bytes = new Option<long>("--bytes") { Description = "数据库当前已分配的存储容量（字节）；设为 0 表示跳过容量判断", Required = true };
        var command = new Command(
            "set-messaging-capacity",
            "更新数据库已分配的存储容量，供消息写入的容量保护判断可用空间；通常由部署执行机读取 RDS 容量后定期调用，超过 24 小时未更新将触发告警")
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

            // 参数校验未通过归为参数错误（退出码 2），其余归为执行失败（退出码 1）
            var reasons = result.Error.FieldErrors.Count > 0 ? string.Join("；", result.Error.FieldErrors.Select(f => f.Message)) : result.Error.Message;
            await context.Output.WriteLineAsync($"失败：{reasons}");
            return result.Error.Type == ErrorType.Validation ? OperatorApp.UsageExitCode : 1;
        }));
        return command;
    }
}
