using Amazon.S3;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Kmic.BuildingBlocks.Testing.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Files;

public sealed class S3FileStorageTests : FileStorageContractTests, IClassFixture<MinioFixture>, IAsyncLifetime
{
    private readonly S3FileStorage _storage;
    private readonly MinioFixture _minio;
    private readonly string _bucket = "kmic-" + Guid.CreateVersion7().ToString("N")[..20];

    public S3FileStorageTests(MinioFixture minio)
    {
        _minio = minio;
        _storage = new S3FileStorage(minio.OptionsFor(_bucket), TimeProvider.System);
    }

    protected override IFileStorage Storage => _storage;

    // 每个测试开始前创建自己的桶
    public async ValueTask InitializeAsync()
    {
        using var admin = new AmazonS3Client(
            new Amazon.Runtime.BasicAWSCredentials(_minio.OptionsFor(_bucket).AccessKeyId, _minio.OptionsFor(_bucket).SecretAccessKey),
            new AmazonS3Config { ServiceURL = _minio.ServiceUrl, ForcePathStyle = true });
        await admin.PutBucketAsync(_bucket, Ct);
    }

    [Fact]
    public async Task PresignedUrl_DownloadsWithTheGivenFileName()
    {
        await Storage.WriteAsync("test/presign", new MemoryStream([1, 2, 3, 4]), "application/pdf", Ct);

        var url = await Storage.GetDownloadUrlAsync("test/presign", TimeSpan.FromMinutes(5), "合同 一.pdf", Ct);

        await using var provider = BuildHttp();
        using var http = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
        using var response = await http.GetAsync(url, Ct);
        response.EnsureSuccessStatusCode();
        (await response.Content.ReadAsByteArrayAsync(Ct)).ShouldBe([1, 2, 3, 4]);
        response.Content.Headers.ContentDisposition!.FileNameStar.ShouldBe("合同 一.pdf");
    }

    private static ServiceProvider BuildHttp()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        return services.BuildServiceProvider();
    }

    public ValueTask DisposeAsync()
    {
        _storage.Dispose();
        return ValueTask.CompletedTask;
    }
}
