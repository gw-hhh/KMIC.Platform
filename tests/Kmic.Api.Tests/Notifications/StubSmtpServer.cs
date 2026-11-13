using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 测试用的最小 SMTP 服务器，记下收到的邮件
/// </summary>
/// <remarks>
/// 监听本机随机端口，不加密、不认证，也不声明任何扩展，所以 MailKit 会把正文编码成 7 位安全的格式。
/// 只认 EHLO、MAIL、RCPT、DATA、RSET、NOOP、QUIT；对收件人的回复由 <see cref="RcptReply" /> 指定，用来模拟拒收和暂时不可用
/// </remarks>
public sealed class StubSmtpServer : IAsyncDisposable
{
    private static readonly UTF8Encoding _utf8 = new(encoderShouldEmitUTF8Identifier: false);

    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _stopping = new();
    private readonly ConcurrentBag<Task> _sessions = [];
    private readonly Task _accepting;
    private volatile string _rcptReply = "250 OK";

    /// <summary>
    /// 开始监听
    /// </summary>
    public StubSmtpServer()
    {
        _listener.Start();
        _accepting = AcceptAsync();
    }

    /// <summary>
    /// 监听的端口
    /// </summary>
    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    /// <summary>
    /// 收到的邮件，按收到的先后排列
    /// </summary>
    public ConcurrentQueue<ReceivedMail> Mails { get; } = new();

    /// <summary>
    /// 对每个收件人的回复，以三位状态码开头；默认接受
    /// </summary>
    public string RcptReply
    {
        get => _rcptReply;
        set => _rcptReply = value;
    }

    /// <summary>
    /// 清掉收到的邮件，收件人回复恢复为接受
    /// </summary>
    public void Reset()
    {
        Mails.Clear();
        _rcptReply = "250 OK";
    }

    /// <summary>
    /// 停止监听，结束还没断开的会话
    /// </summary>
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
            // 关闭服务器时结束监听
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
                // 客户端断开或服务器关闭，会话到此为止
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
            // 行首的 . 在传输时多写了一个，这里还原
            data.Append(line.StartsWith("..", StringComparison.Ordinal) ? line[1..] : line).Append("\r\n");
        }

        Mails.Enqueue(new ReceivedMail(transaction.From, [.. transaction.Recipients], data.ToString()));
        transaction.Begin(string.Empty);
        await ReplyAsync(writer, "250 OK queued", ct);
    }

    /// <summary>
    /// 一次邮件事务：MAIL 开始，RCPT 累加收件人，DATA 结束
    /// </summary>
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
