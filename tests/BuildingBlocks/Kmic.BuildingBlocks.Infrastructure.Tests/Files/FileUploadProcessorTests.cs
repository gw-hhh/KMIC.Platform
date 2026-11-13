using System.Security.Cryptography;
using System.Text;
using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Files;

public sealed class FileUploadProcessorTests : IDisposable
{
    private static readonly UploadProfile _profile = new(
        "BuildingBlocks.Scan", "扫描件", "BuildingBlocks", MaxBytes: 10_000, [FileType.Pdf, FileType.Png], "BuildingBlocks.Scan.Edit");

    private readonly string _root = Path.Combine(Path.GetTempPath(), "kmic-upload-" + Guid.CreateVersion7().ToString("N"));
    private readonly LocalFileStorage _storage;
    private readonly FileUploadProcessor _processor;

    public FileUploadProcessorTests()
    {
        _storage = new LocalFileStorage(_root);
        _processor = new FileUploadProcessor(_storage);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static byte[] Pdf(int size) => [.. "%PDF-1.7\n"u8, .. Enumerable.Range(0, size - 9).Select(i => (byte)('a' + (i % 26)))];

    private async Task<List<string>> StoredKeysAsync()
    {
        var keys = new List<string>();
        await foreach (var key in _storage.ListAsync(string.Empty, Ct))
        {
            keys.Add(key);
        }

        return keys;
    }

    [Theory]
    [InlineData(100)]
    [InlineData(4096)]
    [InlineData(4097)]
    [InlineData(10_000)]
    public async Task APdfWithinTheLimit_IsStored_WithItsRealSizeAndHash(int size)
    {
        var content = Pdf(size);

        var result = await _processor.StoreAsync(_profile, "合同.PDF", new MemoryStream(content), "env/2026/11/abc", Ct);

        result.Outcome.ShouldBe(UploadOutcome.Stored);
        var file = result.File.ShouldNotBeNull();
        file.Size.ShouldBe(size);
        file.Sha256.ShouldBe(Convert.ToHexStringLower(SHA256.HashData(content)));
        file.Type.Code.ShouldBe(FileType.Pdf);
        file.Stored.Key.ShouldBe("env/2026/11/abc");
        await using var stored = await _storage.OpenReadAsync("env/2026/11/abc", Ct);
        using var copy = new MemoryStream();
        await stored.CopyToAsync(copy, Ct);
        copy.ToArray().ShouldBe(content);
    }

    [Fact]
    public async Task OneByteOverTheLimit_IsTooLarge_AndNothingIsLeftInTheStorage()
    {
        var result = await _processor.StoreAsync(_profile, "a.pdf", new MemoryStream(Pdf(10_001)), "env/big", Ct);

        result.Outcome.ShouldBe(UploadOutcome.TooLarge);
        result.File.ShouldBeNull();
        (await StoredKeysAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task ATooLargeBodyThatKeepsComing_IsCutOffWithoutReadingItAll()
    {
        await using var endless = new EndlessStream();

        var result = await _processor.StoreAsync(_profile, "a.pdf", endless, "env/endless", Ct);

        result.Outcome.ShouldBe(UploadOutcome.TooLarge);
        endless.BytesServed.ShouldBeLessThan(_profile.MaxBytes + 1_000_000, "超过上限就停止读取，不会把一个无限的流读完");
        (await StoredKeysAsync()).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("a.exe", "%PDF-1.4 content")]
    [InlineData("a.pdf", "这不是 PDF")]
    [InlineData("a.jpg", "%PDF-1.4 content")]
    [InlineData("a.txt", "纯文本内容")]
    public async Task ATypeThatDoesNotMatchOrIsNotAllowedByTheProfile_IsRefusedBeforeAnythingIsWritten(string fileName, string content)
    {
        var result = await _processor.StoreAsync(_profile, fileName, new MemoryStream(Encoding.UTF8.GetBytes(content)), "env/refused", Ct);

        result.Outcome.ShouldBe(UploadOutcome.TypeNotAllowed);
        (await StoredKeysAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task AnEmptyFile_IsRefused()
    {
        var result = await _processor.StoreAsync(_profile, "a.pdf", new MemoryStream(), "env/empty", Ct);

        result.Outcome.ShouldBe(UploadOutcome.Empty);
        (await StoredKeysAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task ABrokenConnectionHalfwayThrough_LeavesNoHalfWrittenObject_AndTheErrorPropagates()
    {
        await using var broken = new BrokenStream(Pdf(6000), failAfter: 5000);

        await Should.ThrowAsync<IOException>(async () => await _processor.StoreAsync(_profile, "a.pdf", broken, "env/broken", Ct));

        (await StoredKeysAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Cancelling_StopsTheUpload_AndLeavesNoObject()
    {
        using var cancel = new CancellationTokenSource();
        await using var slow = new CancelAfterStream(Pdf(8000), cancel, after: 5000);

        await Should.ThrowAsync<OperationCanceledException>(async () => await _processor.StoreAsync(_profile, "a.pdf", slow, "env/cancelled", cancel.Token));

        (await StoredKeysAsync()).ShouldBeEmpty();
    }

    // 客户端一直发，发不完
    private sealed class EndlessStream : Stream
    {
        public long BytesServed { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var offset = 0;
            if (BytesServed == 0)
            {
                "%PDF-1.7\n"u8.CopyTo(buffer.Span);
                offset = 9;
            }

            buffer.Span[offset..].Fill((byte)'x');
            BytesServed += buffer.Length;
            await Task.Yield();
            return buffer.Length;
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class BrokenStream(byte[] content, int failAfter) : Stream
    {
        private readonly MemoryStream _inner = new(content);

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            _inner.Position >= failAfter ? throw new IOException("连接断开") : _inner.ReadAsync(buffer[..Math.Min(buffer.Length, failAfter - (int)_inner.Position)], cancellationToken);

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

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
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    private sealed class CancelAfterStream(byte[] content, CancellationTokenSource cancel, int after) : Stream
    {
        private readonly MemoryStream _inner = new(content);

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_inner.Position >= after)
            {
                await cancel.CancelAsync();
                cancellationToken.ThrowIfCancellationRequested();
            }

            return await _inner.ReadAsync(buffer[..Math.Min(buffer.Length, after - (int)_inner.Position + 1)], cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

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
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
