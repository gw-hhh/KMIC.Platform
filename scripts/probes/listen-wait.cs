// 验证 Npgsql 四种监听等待方式是否能按时返回。
// Windows 平台下异步等待（WaitAsync）曾存在不响应超时与取消的问题，因此监听循环目前在专用线程中调用同步 Wait；
// 升级 Npgsql 后可运行此探测脚本，若异步方式均能正常超时/取消返回，即可将监听循环改回异步实现。
// Windows：先 docker run -d --rm --name kmic-listen -e POSTGRES_PASSWORD=x -p 55432:5432 postgres:18，再 dotnet run scripts/probes/listen-wait.cs
// Linux：把连接串改为 Host=127.0.0.1;Port=5432，在 sdk:10.0 容器中以 --network container:kmic-listen 运行
#:package Npgsql
#:property RestorePackagesWithLockFile=false
#:property PublishAot=false
using System.Diagnostics;
using Npgsql;

await using var source = NpgsqlDataSource.Create("Host=localhost;Port=55432;Username=postgres;Password=x");

async Task<NpgsqlConnection> ListenAsync()
{
    var connection = await source.OpenConnectionAsync();
    await using var cmd = new NpgsqlCommand("LISTEN probe", connection);
    await cmd.ExecuteNonQueryAsync();
    return connection;
}

async Task RunAsync(string label, Func<NpgsqlConnection, Task<bool>> wait)
{
    var connection = await ListenAsync();
    var sw = Stopwatch.StartNew();
    var task = wait(connection);
    var done = await Task.WhenAny(task, Task.Delay(5000)) == task;
    string result;
    try
    {
        result = done ? $"returned {await task} after {sw.ElapsedMilliseconds} ms" : "still waiting after 5 s";
    }
    catch (OperationCanceledException e)
    {
        result = $"{e.GetType().Name} after {sw.ElapsedMilliseconds} ms";
    }

    Console.WriteLine($"{label}: {result}");
}

await RunAsync("WaitAsync(TimeSpan 500ms)", c => c.WaitAsync(TimeSpan.FromMilliseconds(500)));
await RunAsync("WaitAsync(int 500)", c => c.WaitAsync(500));
await RunAsync("WaitAsync(CancellationToken 500ms)", async c =>
{
    using var cts = new CancellationTokenSource(500);
    await c.WaitAsync(cts.Token);
    return true;
});
await RunAsync("Wait(int 500) 同步", c => Task.Run(() => c.Wait(500)));
Environment.Exit(0);
