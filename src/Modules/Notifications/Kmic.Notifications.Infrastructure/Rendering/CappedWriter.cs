using System.Text;

namespace Kmic.Notifications.Infrastructure.Rendering;

/// <summary>
/// 超过输出大小或渲染时间上限就抛异常的文本写入器
/// </summary>
/// <remarks>
/// Fluid 没有自带的挂钟时间限制，只有步数上限；渲染往这里写的每一次都检查已经用了多久，
/// 写个不停的循环会在到点时被打断，不写输出的循环由步数上限兜底
/// </remarks>
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
            throw new InvalidOperationException($"输出超过了 {maxChars} 个字符的上限");
        }

        if (time.GetElapsedTime(_started) > timeout)
        {
            throw new TimeoutException($"渲染超过了 {timeout.TotalMilliseconds:0} 毫秒的上限");
        }
    }
}
