using System.CommandLine;
using Microsoft.Extensions.Configuration;

namespace Kmic.Operator;

/// <summary>
/// 运维命令
/// </summary>
/// <remarks>
/// 命令随需要它的模块一起加：首个管理员初始化与应急重置随 Identity 模块。
/// 退出码：0 成功；1 执行失败；2 命令行写错了。
/// 密码从标准输入读第一行，不出现在命令行参数和 shell 历史里；连接串从配置读（Kmic:Operator:ConnectionString）
/// </remarks>
internal static class OperatorApp
{
    public const string ConnectionStringKey = "Kmic:Operator:ConnectionString";

    public const int UsageExitCode = 2;

    public static async Task<int> RunAsync(string[] args, IConfiguration configuration, TextReader input, TextWriter output, TimeProvider time, CancellationToken ct)
    {
        var context = new OperatorContext(configuration, input, output, time);
        var root = new RootCommand("Kmic 运维命令：人工或部署流程触发的一次性操作")
        {
            IdentityCommands.Build(context),
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
}
