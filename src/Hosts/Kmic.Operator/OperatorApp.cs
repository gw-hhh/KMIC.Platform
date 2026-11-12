using System.CommandLine;

namespace Kmic.Operator;

/// <summary>
/// 运维命令
/// </summary>
/// <remarks>
/// 目前只有骨架：入口和退出码。命令随需要它的模块一起加，同时加上运维账号的连接串配置（Kmic:Operator:ConnectionString）
/// 和模块清单：首个管理员初始化（identity bootstrap）随 Identity 模块，消息容量的写入随消息准入的运行配置。
/// 退出码：0 成功；1 执行失败；2 命令行写错了
/// </remarks>
internal static class OperatorApp
{
    public const int UsageExitCode = 2;

    public static async Task<int> RunAsync(string[] args, TextWriter output, CancellationToken ct)
    {
        var root = new RootCommand("Kmic 运维命令：人工或部署流程触发的一次性操作");

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
