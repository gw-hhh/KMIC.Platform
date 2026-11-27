using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Xunit;

namespace Kmic.BuildingBlocks.Testing.Storage;

/// <summary>测试用 MinIO 容器夹具，供 S3 存储测试共用（可通过环境变量 KMIC_TEST_MINIO_IMAGE 覆盖默认镜像）。</summary>
public sealed class MinioFixture : IAsyncLifetime
{
    private const string DefaultImage = "kmic/minio:RELEASE.2025-10-15T17-29-55Z";
    private const string AccessKey = "kmic-test";

    // 每次启动容器时随机生成密钥，避免硬编码
    private readonly string _secretKey = Guid.CreateVersion7().ToString("N");
    private readonly IContainer _container;

    public MinioFixture() =>
        _container = new ContainerBuilder(Environment.GetEnvironmentVariable("KMIC_TEST_MINIO_IMAGE") ?? DefaultImage)
            .WithName(TestContainerNames.For("minio"))
            .WithLabel(TestContainerNames.GroupLabel, TestContainerNames.Group)
            .WithPortBinding(TestPorts.Free(), 9000)
            .WithEnvironment("MINIO_ROOT_USER", AccessKey)
            .WithEnvironment("MINIO_ROOT_PASSWORD", _secretKey)
            .WithCommand("server", "/data")
            .WithCreateParameterModifier(parameters => parameters.User = "0:0")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request.ForPath("/minio/health/ready").ForPort(9000)))
            .Build();

    /// <summary>容器内 MinIO 服务的访问地址。</summary>
    public string ServiceUrl => $"http://{_container.Hostname}:{_container.GetMappedPublicPort(9000)}";

    /// <summary>生成连接指定存储桶的 S3 配置。</summary>
    public S3FileStorageOptions OptionsFor(string bucket) => new()
    {
        ServiceUrl = ServiceUrl,
        Bucket = bucket,
        AccessKeyId = AccessKey,
        SecretAccessKey = _secretKey,
        ForcePathStyle = true,
        PartSizeBytes = 5 * 1024 * 1024,
    };

    public async ValueTask InitializeAsync() => await _container.StartAsync(TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
