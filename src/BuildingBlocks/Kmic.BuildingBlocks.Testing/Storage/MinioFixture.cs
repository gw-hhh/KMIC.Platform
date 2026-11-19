using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Xunit;

namespace Kmic.BuildingBlocks.Testing.Storage;

/// <summary>
/// 测试用的 MinIO 容器，S3 存储的测试共用
/// </summary>
/// <remarks>
/// 换镜像设环境变量 KMIC_TEST_MINIO_IMAGE
/// </remarks>
public sealed class MinioFixture : IAsyncLifetime
{
    private const string DefaultImage = "kmic/minio:RELEASE.2025-10-15T17-29-55Z";
    private const string AccessKey = "kmic-test";

    // 容器每次启动都随机生成，不写进代码
    private readonly string _secretKey = Guid.CreateVersion7().ToString("N");
    private readonly IContainer _container;

    /// <summary>
    /// 准备容器，还没有启动
    /// </summary>
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

    /// <summary>
    /// 容器里的 MinIO 地址
    /// </summary>
    public string ServiceUrl => $"http://{_container.Hostname}:{_container.GetMappedPublicPort(9000)}";

    /// <summary>
    /// 创建一个桶并返回连到它的配置
    /// </summary>
    public S3FileStorageOptions OptionsFor(string bucket) => new()
    {
        ServiceUrl = ServiceUrl,
        Bucket = bucket,
        AccessKeyId = AccessKey,
        SecretAccessKey = _secretKey,
        ForcePathStyle = true,
        PartSizeBytes = 5 * 1024 * 1024,
    };

    /// <summary>
    /// 启动容器
    /// </summary>
    public async ValueTask InitializeAsync() => await _container.StartAsync(TestContext.Current.CancellationToken);

    /// <summary>
    /// 销毁容器
    /// </summary>
    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
