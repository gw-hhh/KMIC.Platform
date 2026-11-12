using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Kmic.Operator;

/// <summary>
/// 各个命令共用的环境：配置、标准输入、输出和时间
/// </summary>
internal sealed class OperatorContext(IConfiguration configuration, TextReader input, TextWriter output, TimeProvider time)
{
    public TextWriter Output => output;

    public TimeProvider Time => time;

    public string ConnectionString() =>
        configuration[OperatorApp.ConnectionStringKey] is { Length: > 0 } connection
            ? KmicConnectionString.Normalize(connection)
            : throw new InvalidOperationException($"没有配置运维账号的连接串 {OperatorApp.ConnectionStringKey}。");

    // 密码从标准输入读第一行；输入已经结束或是空行都当作没有给
    public async Task<string> ReadPasswordAsync()
    {
        var line = await input.ReadLineAsync();
        return string.IsNullOrEmpty(line) ? throw new InvalidOperationException("标准输入里没有读到密码：请用管道或重定向给出，不要写在命令行参数里。") : line;
    }

    // 连不上数据库和参数不合规都说明原因并返回 1，不让异常堆栈直接打到部署日志里
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
