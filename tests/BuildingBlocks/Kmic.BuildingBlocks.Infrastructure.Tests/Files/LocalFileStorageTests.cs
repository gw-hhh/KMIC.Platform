using Kmic.BuildingBlocks.Infrastructure.Files;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Files;

public sealed class LocalFileStorageTests : FileStorageContractTests, IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "kmic-files-" + Guid.CreateVersion7().ToString("N"));
    private readonly LocalFileStorage _storage;

    public LocalFileStorageTests() => _storage = new LocalFileStorage(_root);

    protected override IFileStorage Storage => _storage;

    [Theory]
    [InlineData("../escape")]
    [InlineData("a/../../escape")]
    [InlineData("/absolute")]
    [InlineData("back\\slash")]
    [InlineData("c:/drive")]
    public async Task UnsafeKey_Rejected(string key) =>
        await Should.ThrowAsync<ArgumentException>(() => Storage.WriteAsync(key, new MemoryStream([1]), "application/octet-stream", Ct));

    [Fact]
    public async Task PresignedUrl_NotSupported() =>
        await Should.ThrowAsync<NotSupportedException>(() => Storage.GetDownloadUrlAsync("any", TimeSpan.FromMinutes(1), "a.pdf", Ct));

    [Fact]
    public async Task FailedWrite_LeavesNoHalfWrittenFile()
    {
        var thrown = await Should.ThrowAsync<IOException>(() => Storage.WriteAsync("test/broken", new BrokenStream(), "application/octet-stream", Ct));

        thrown.Message.ShouldContain("读不下去");
        var keys = new List<string>();
        await foreach (var key in Storage.ListAsync("", Ct))
        {
            keys.Add(key);
        }

        keys.ShouldBeEmpty();
    }

    [Fact]
    public void Key_FollowsEnvironmentYearMonthAndFileId()
    {
        var id = Guid.Parse("0197a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5b");

        FileStorageKeys.For("prod", id, new DateTimeOffset(2026, 11, 5, 23, 0, 0, TimeSpan.FromHours(8))).ShouldBe("prod/2026/11/0197a1b2c3d47e5f8a9b0c1d2e3f4a5b");
        Should.Throw<ArgumentException>(() => FileStorageKeys.For("a/b", id, DateTimeOffset.UnixEpoch));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private sealed class BrokenStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new IOException("读不下去");

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new IOException("读不下去");

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => throw new IOException("读不下去");

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
