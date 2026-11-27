using Kmic.Operator;
using Microsoft.Extensions.Hosting;

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
};

// 连接字符串仅从配置读取，不接收命令行传参，避免泄露至进程列表或日志
var builder = Host.CreateApplicationBuilder();
return await OperatorApp.RunAsync(args, builder.Configuration, Console.In, Console.Out, TimeProvider.System, cancellation.Token);
