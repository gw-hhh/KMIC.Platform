namespace Kmic.BuildingBlocks.Testing;

/// <summary>
/// 测试容器的命名：在 Docker Desktop 里一眼能看出是什么、属于谁
/// </summary>
/// <remarks>
/// 容器名只能用英文字母、数字、点、下划线和短横线，不能用中文。名字里带随机后缀，同时跑两次测试也不会重名
/// </remarks>
internal static class TestContainerNames
{
    /// <summary>
    /// Docker Desktop 按这个标签把同一组容器归在一起显示
    /// </summary>
    public const string GroupLabel = "com.docker.compose.project";

    /// <summary>
    /// 所有测试容器所在的分组名
    /// </summary>
    public const string Group = "kmic-test";

    /// <summary>
    /// 为某种用途生成容器名，如 kmic-test-postgres-3f9a1c2e
    /// </summary>
    /// <param name="purpose">用途，如 postgres、minio</param>
    public static string For(string purpose) => $"{Group}-{purpose}-{Guid.CreateVersion7().ToString("N")[^8..]}";
}
