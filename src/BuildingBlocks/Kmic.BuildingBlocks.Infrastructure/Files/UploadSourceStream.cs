using System.Security.Cryptography;

namespace Kmic.BuildingBlocks.Infrastructure.Files;

// 单次顺序只读校验流：实时统计字节数并计算 SHA-256，超出上限立即中断
internal sealed class UploadSourceStream(ReadOnlyMemory<byte> header, Stream rest, long maxBytes) : Stream
{
    private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    private int _headerRead;

    public long BytesRead { get; private set; }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => BytesRead;
        set => throw new NotSupportedException();
    }

    // 返回小写十六进制 SHA-256 摘要并重置哈希器，仅可调用一次
    public string FinishHash() => Convert.ToHexStringLower(_hash.GetHashAndReset());

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

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

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

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

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
