using System.Security.Cryptography;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Files;

/// <summary>
/// 本地和 S3 存储通过同一套契约测试。每个测试用自己的前缀，互不影响
/// </summary>
public abstract class FileStorageContractTests
{
    private readonly string _prefix = "test/" + Guid.CreateVersion7().ToString("N") + "/";

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    protected abstract IFileStorage Storage { get; }

    [Fact]
    public async Task WriteThenRead_SameBytes()
    {
        var bytes = RandomNumberGenerator.GetBytes(1000);

        var stored = await Storage.WriteAsync(_prefix + "small", new MemoryStream(bytes), "application/octet-stream", Ct);

        stored.Key.ShouldBe(_prefix + "small");
        (await ReadAllAsync(_prefix + "small")).ShouldBe(bytes);
    }

    [Fact]
    public async Task LargeNonSeekableStream_WrittenInPartsAndReadBackIntact()
    {
        const int Size = 12 * 1024 * 1024;
        await using var source = new NonSeekableStream(Size);

        await Storage.WriteAsync(_prefix + "large", source, "application/octet-stream", Ct);

        await using var stored = await Storage.OpenReadAsync(_prefix + "large", Ct);
        var hash = await SHA256.HashDataAsync(stored, Ct);
        hash.ShouldBe(source.Hash());
    }

    [Fact]
    public async Task Delete_RemovesObject_MissingIsNotAnError()
    {
        await Storage.WriteAsync(_prefix + "gone", new MemoryStream([1, 2, 3]), "application/octet-stream", Ct);

        await Storage.DeleteAsync(_prefix + "gone", Ct);
        await Storage.DeleteAsync(_prefix + "never-existed", Ct);

        await Should.ThrowAsync<FileNotFoundException>(() => Storage.OpenReadAsync(_prefix + "gone", Ct));
    }

    [Fact]
    public async Task ReadMissing_ThrowsFileNotFound() =>
        await Should.ThrowAsync<FileNotFoundException>(() => Storage.OpenReadAsync(_prefix + "missing", Ct));

    [Fact]
    public async Task Exists_TrueAfterWrite_FalseAfterDeleteOrWhenNeverWritten()
    {
        await Storage.WriteAsync(_prefix + "here", new MemoryStream([1]), "application/octet-stream", Ct);

        (await Storage.ExistsAsync(_prefix + "here", null, Ct)).ShouldBeTrue();
        (await Storage.ExistsAsync(_prefix + "never", null, Ct)).ShouldBeFalse();
        await Storage.DeleteAsync(_prefix + "here", Ct);
        (await Storage.ExistsAsync(_prefix + "here", null, Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task List_ReturnsOnlyKeysUnderThePrefix()
    {
        await Storage.WriteAsync(_prefix + "a/1", new MemoryStream([1]), "application/octet-stream", Ct);
        await Storage.WriteAsync(_prefix + "a/2", new MemoryStream([2]), "application/octet-stream", Ct);
        await Storage.WriteAsync(_prefix + "b/1", new MemoryStream([3]), "application/octet-stream", Ct);

        var keys = new List<string>();
        await foreach (var key in Storage.ListAsync(_prefix + "a/", Ct))
        {
            keys.Add(key);
        }

        keys.Order(StringComparer.Ordinal).ShouldBe([_prefix + "a/1", _prefix + "a/2"]);
    }

    protected async Task<byte[]> ReadAllAsync(string key)
    {
        await using var stream = await Storage.OpenReadAsync(key, Ct);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, Ct);
        return buffer.ToArray();
    }

    // 不能回到开头、也不知道总长度的流，模拟请求体
    private sealed class NonSeekableStream(int length) : Stream
    {
        private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        private int _position;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public byte[] Hash() => _hash.GetHashAndReset();

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var take = Math.Min(count, length - _position);
            for (var i = 0; i < take; i++)
            {
                buffer[offset + i] = (byte)((_position + i) * 31);
            }

            _hash.AppendData(buffer, offset, take);
            _position += take;
            return take;
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
    }
}
