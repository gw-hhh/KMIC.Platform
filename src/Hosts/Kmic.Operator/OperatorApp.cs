using System.CommandLine;
using Microsoft.Extensions.Configuration;

namespace Kmic.Operator;

/// <summary>
/// 运维命令
/// </summary>
/// <remarks>
/// 退出码：0 成功，1 执行失败，2 参数错误；密码从标准输入读第一行
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

        // 参数解析失败统一返回 2，与执行失败的退出码 1 区分
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
