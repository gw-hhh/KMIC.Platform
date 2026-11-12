using Kmic.Operator;
using Microsoft.Extensions.Hosting;

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
};

// 连接串只从配置读（环境变量 Kmic__Operator__ConnectionString 或密钥管理注入），不接受命令行参数，免得出现在进程列表和日志里
var builder = Host.CreateApplicationBuilder();
return await OperatorApp.RunAsync(args, builder.Configuration, Console.In, Console.Out, TimeProvider.System, cancellation.Token);
