using System.Security.Cryptography;

namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 交给存储写入的只读流：先给已经读出来识别类型的文件头，再接着读上传的内容；
/// 边读边算 SHA-256、边数字节，超过上限就中断
/// </summary>
/// <remarks>
/// 只能顺序读一遍。写存储的一端怎么分段、怎么重试都看不到这里的细节
/// </remarks>
internal sealed class UploadSourceStream(ReadOnlyMemory<byte> header, Stream rest, long maxBytes) : Stream
{
    private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    private int _headerRead;

    /// <summary>
    /// 到现在读了多少字节
    /// </summary>
    public long BytesRead { get; private set; }

    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc />
    public override long Position
    {
        get => BytesRead;
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// 读完之后的 SHA-256，小写十六进制；只能取一次
    /// </summary>
    public string FinishHash() => Convert.ToHexStringLower(_hash.GetHashAndReset());

    /// <inheritdoc />
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (buffer.IsEmpty)
        {
            return 0;
        }

        int read;
        if (_headerRead < header.Length)
        {
            read = Math.Min(buffer.Length, header.Length - _headerRead);
            header.Slice(_headerRead, read).CopyTo(buffer);
            _headerRead += read;
        }
        else
        {
            read = await rest.ReadAsync(buffer, cancellationToken);
        }

        Count(buffer.Span[..read]);
        return read;
    }

    /// <inheritdoc />
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return 0;
        }

        int read;
        if (_headerRead < header.Length)
        {
            read = Math.Min(buffer.Length, header.Length - _headerRead);
            header.Span.Slice(_headerRead, read).CopyTo(buffer);
            _headerRead += read;
        }
        else
        {
            read = rest.Read(buffer);
        }

        Count(buffer[..read]);
        return read;
    }

    /// <inheritdoc />
    public override void Flush()
    {
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hash.Dispose();
        }

        base.Dispose(disposing);
    }

    private void Count(ReadOnlySpan<byte> read)
    {
        BytesRead += read.Length;
        if (BytesRead > maxBytes)
        {
            throw new UploadTooLargeException(maxBytes);
        }

        _hash.AppendData(read);
    }
}
