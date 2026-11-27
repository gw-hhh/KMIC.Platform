using System.CommandLine;
using Microsoft.Extensions.Configuration;

namespace Kmic.Operator;

// 退出码：0 成功，1 失败，2 参数错误；密码从标准输入读取首行
internal static class OperatorApp
{
    public const string ConnectionStringKey = "Kmic:Operator:ConnectionString";

    public const int UsageExitCode = 2;

    public static async Task<int> RunAsync(string[] args, IConfiguration configuration, TextReader input, TextWriter output, TimeProvider time, CancellationToken ct)
    {
        var context = new OperatorContext(configuration, input, output, time);
        var root = new RootCommand("Kmic 运维命令：供人工维护或部署流程调用的命令行工具")
        {
            IdentityCommands.Build(context),
            OperationsCommands.Build(context),
        };

        // 参数解析失败统一返回退出码 2，以区分执行失败（1）
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
