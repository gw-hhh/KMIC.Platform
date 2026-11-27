using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Kmic.Operator;

internal sealed class OperatorContext(IConfiguration configuration, TextReader input, TextWriter output, TimeProvider time)
{
    public TextWriter Output => output;

    public TimeProvider Time => time;

    public string ConnectionString() =>
        configuration[OperatorApp.ConnectionStringKey] is { Length: > 0 } connection
            ? KmicConnectionString.Normalize(connection)
            : throw new InvalidOperationException($"未配置运维账号连接字符串 {OperatorApp.ConnectionStringKey}。");

    // 密码仅读取标准输入首行，输入结束或空行均视为缺失
    public async Task<string> ReadPasswordAsync()
    {
        var line = await input.ReadLineAsync();
        return string.IsNullOrEmpty(line) ? throw new InvalidOperationException("未从标准输入读取到密码，请通过管道或重定向提供，勿放入命令行参数。") : line;
    }

    // 只输出错误原因并返回 1，避免将异常堆栈写入部署日志
    public async Task<int> GuardedAsync(Func<Task<int>> run)
    {
        try
        {
            return await run();
        }
        catch (Exception exception) when (exception is InvalidOperationException or NpgsqlException or ArgumentException)
        {
            await output.WriteLineAsync($"失败：{exception.Message}");
            return 1;
        }
    }
}
