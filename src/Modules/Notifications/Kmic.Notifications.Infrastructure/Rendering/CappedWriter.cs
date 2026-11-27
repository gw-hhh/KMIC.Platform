using System.Text;

namespace Kmic.Notifications.Infrastructure.Rendering;

internal sealed class CappedWriter(int maxChars, TimeSpan timeout, TimeProvider time) : TextWriter
{
    private readonly StringBuilder _text = new();
    private readonly long _started = time.GetTimestamp();

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value) => Append(value);

    public override void Write(string? value) => Append(value);

    public override void Write(char[] buffer, int index, int count) => Append(buffer.AsSpan(index, count));

    public override void Write(ReadOnlySpan<char> buffer) => Append(buffer);

    public override Task WriteAsync(char value)
    {
        Append(value);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(string? value)
    {
        Append(value);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(char[] buffer, int index, int count)
    {
        Append(buffer.AsSpan(index, count));
        return Task.CompletedTask;
    }

    public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        Append(buffer.Span);
        return Task.CompletedTask;
    }

    public override string ToString() => _text.ToString();

    private void Append(char value)
    {
        Reserve(1);
        _text.Append(value);
    }

    private void Append(string? value)
    {
        if (value is null)
        {
            return;
        }

        Reserve(value.Length);
        _text.Append(value);
    }

    private void Append(ReadOnlySpan<char> value)
    {
        Reserve(value.Length);
        _text.Append(value);
    }

    private void Reserve(int count)
    {
        if (_text.Length + count > maxChars)
        {
            throw new InvalidOperationException($"输出长度超过上限（{maxChars} 个字符）");
        }

        if (time.GetElapsedTime(_started) > timeout)
        {
            throw new TimeoutException($"渲染耗时超过上限（{timeout.TotalMilliseconds:0} 毫秒）");
        }
    }
}
