using System.CommandLine;
using Kmic.Identity.Infrastructure.Operations;

namespace Kmic.Operator;

/// <summary>
/// Identity 模块的运维命令
/// </summary>
internal static class IdentityCommands
{
    public static Command Build(OperatorContext context) =>
        new("identity", "Identity 模块的运维命令")
        {
            Bootstrap(context),
            ResetAdminPassword(context),
        };

    private static Command Bootstrap(OperatorContext context)
    {
        var account = new Option<string>("--account") { Description = "管理员账号", Required = true };
        var name = new Option<string>("--name") { Description = "管理员姓名", Required = true };
        var rootUnit = new Option<string?>("--root-unit-name") { Description = "组织树根单元的名称，不传用“总部”" };
        var command = new Command("bootstrap", "初始化首个管理员，创建内置管理员角色、账号及角色授予和组织树根单元。仅系统尚无普通用户时可执行；密码从标准输入读取")
        {
            account,
            name,
            rootUnit,
        };
        command.SetAction((parse, ct) => context.GuardedAsync(async () =>
        {
            var password = await context.ReadPasswordAsync();
            var result = await new IdentityBootstrapper().BootstrapAsync(
                context.ConnectionString(), parse.GetRequiredValue(account), parse.GetRequiredValue(name), password, parse.GetValue(rootUnit), context.Time.GetUtcNow(), ct);
            await context.Output.WriteLineAsync(result.IsSuccess ? "已创建首个管理员。首次登录后要先修改密码。" : $"失败：{result.Error.Message}（{result.Error.Code}）");
            return result.IsSuccess ? 0 : 1;
        }));
        return command;
    }

    private static Command ResetAdminPassword(OperatorContext context)
    {
        var account = new Option<string>("--account") { Description = "管理员账号", Required = true };
        var command = new Command("reset-admin-password", "应急重置管理员密码，同时作废相关会话、清除登录限制并记录安全事件。仅限受保护角色持有人；不会启用已禁用账号。新密码从标准输入读取")
        {
            account,
        };
        command.SetAction((parse, ct) => context.GuardedAsync(async () =>
        {
            var password = await context.ReadPasswordAsync();
            var result = await new IdentityBootstrapper().ResetAdministratorPasswordAsync(
                context.ConnectionString(), parse.GetRequiredValue(account), password, context.Time.GetUtcNow(), ct);
            await context.Output.WriteLineAsync(result.IsSuccess ? "已重置。账号下次登录要先修改密码。" : $"失败：{result.Error.Message}（{result.Error.Code}）");
            return result.IsSuccess ? 0 : 1;
        }));
        return command;
    }
}
