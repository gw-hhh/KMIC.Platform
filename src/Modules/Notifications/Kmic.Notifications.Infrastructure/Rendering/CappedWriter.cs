using System.Text;

namespace Kmic.Notifications.Infrastructure.Rendering;

/// <summary>
/// 限制模板输出大小和渲染用时的文本写入器
/// </summary>
/// <remarks>
/// 每次写入时检查实际用时和输出大小；持续输出的循环超限即中断，不输出的循环由 Fluid 步数上限限制
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
