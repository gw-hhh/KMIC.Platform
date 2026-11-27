using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Kmic.Api.Tests.Notifications;

/// <summary>轻量级测试 SMTP 服务器：监听本机随机端口，支持无加密基础命令，可通过 <see cref="RcptReply"/> 模拟拒收或故障</summary>
public sealed class StubSmtpServer : IAsyncDisposable
{
    private static readonly UTF8Encoding _utf8 = new(encoderShouldEmitUTF8Identifier: false);

    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _stopping = new();
    private readonly ConcurrentBag<Task> _sessions = [];
    private readonly Task _accepting;
    private volatile string _rcptReply = "250 OK";

    public StubSmtpServer()
    {
        _listener.Start();
        _accepting = AcceptAsync();
    }

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    public ConcurrentQueue<ReceivedMail> Mails { get; } = new();

    /// <summary>RCPT 阶段响应行（须以三位状态码开头），默认为接受</summary>
    public string RcptReply
    {
        get => _rcptReply;
        set => _rcptReply = value;
    }

    public void Reset()
    {
        Mails.Clear();
        _rcptReply = "250 OK";
    }

    public async ValueTask DisposeAsync()
    {
        await _stopping.CancelAsync();
        _listener.Stop();
        await _accepting;
        await Task.WhenAll(_sessions);
        _stopping.Dispose();
        GC.SuppressFinalize(this);
    }

    private static bool Is(string line, string command) => line.StartsWith(command, StringComparison.OrdinalIgnoreCase);

    private static string AddressIn(string line)
    {
        var start = line.IndexOf('<', StringComparison.Ordinal);
        var end = line.IndexOf('>', StringComparison.Ordinal);
        return start >= 0 && end > start ? line[(start + 1)..end] : line;
    }

    private static async Task ReplyAsync(StreamWriter writer, string text, CancellationToken ct) =>
        await writer.WriteLineAsync(text.AsMemory(), ct);

    private async Task AcceptAsync()
    {
        try
        {
            while (!_stopping.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(_stopping.Token);
                _sessions.Add(Task.Run(() => RunSessionAsync(client, _stopping.Token), _stopping.Token));
            }
        }
        catch (OperationCanceledException)
        {
            // 服务器关闭时正常退出监听循环
        }
    }

    private async Task RunSessionAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        {
            try
            {
                var stream = client.GetStream();
                using var reader = new StreamReader(stream, _utf8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                await using var writer = new StreamWriter(stream, _utf8, leaveOpen: true) { NewLine = "\r\n", AutoFlush = true };
                await ReplyAsync(writer, "220 stub ESMTP", ct);

                var transaction = new Transaction();
                while (await reader.ReadLineAsync(ct) is { } line)
                {
                    if (Is(line, "QUIT"))
                    {
                        await ReplyAsync(writer, "221 bye", ct);
                        return;
                    }

                    if (Is(line, "DATA"))
                    {
                        await ReceiveDataAsync(reader, writer, transaction, ct);
                        continue;
                    }

                    await ReplyAsync(writer, ReplyTo(line, transaction), ct);
                }
            }
            catch (Exception exception) when (exception is IOException or ObjectDisposedException or OperationCanceledException)
            {
                // 客户端断开连接或服务器关闭时结束当前会话
            }
        }
    }

    private string ReplyTo(string line, Transaction transaction)
    {
        if (Is(line, "EHLO") || Is(line, "HELO"))
        {
            return "250 stub";
        }

        if (Is(line, "MAIL FROM:"))
        {
            transaction.Begin(AddressIn(line));
            return "250 OK";
        }

        if (Is(line, "RCPT TO:"))
        {
            var reply = _rcptReply;
            if (reply.StartsWith("250", StringComparison.Ordinal))
            {
                transaction.Recipients.Add(AddressIn(line));
            }

            return reply;
        }

        if (Is(line, "RSET"))
        {
            transaction.Begin(string.Empty);
            return "250 OK";
        }

        return Is(line, "NOOP") ? "250 OK" : "502 command not implemented";
    }

    private async Task ReceiveDataAsync(StreamReader reader, StreamWriter writer, Transaction transaction, CancellationToken ct)
    {
        if (transaction.Recipients.Count == 0)
        {
            await ReplyAsync(writer, "503 need RCPT first", ct);
            return;
        }

        await ReplyAsync(writer, "354 end data with <CR><LF>.<CR><LF>", ct);
        var data = new StringBuilder();
        while (await reader.ReadLineAsync(ct) is { } line && !string.Equals(line, ".", StringComparison.Ordinal))
        {
            // 按 SMTP 透明规则还原行首转义的句点
            data.Append(line.StartsWith("..", StringComparison.Ordinal) ? line[1..] : line).Append("\r\n");
        }

        Mails.Enqueue(new ReceivedMail(transaction.From, [.. transaction.Recipients], data.ToString()));
        transaction.Begin(string.Empty);
        await ReplyAsync(writer, "250 OK queued", ct);
    }

    // 单次邮件传输事务状态（MAIL -> RCPT -> DATA）
    private sealed class Transaction
    {
        public string From { get; private set; } = string.Empty;

        public List<string> Recipients { get; } = [];

        public void Begin(string from)
        {
            From = from;
            Recipients.Clear();
        }
    }
}
