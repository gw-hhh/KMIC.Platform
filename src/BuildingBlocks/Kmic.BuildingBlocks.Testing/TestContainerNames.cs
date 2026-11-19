namespace Kmic.BuildingBlocks.Testing;

/// <summary>
/// 测试容器的命名：在 Docker Desktop 里一眼能看出是什么、属于谁
/// </summary>
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
    public static string For(string purpose) => $"{Group}-{purpose}-{Guid.CreateVersion7().ToString("N")[^8..]}";
}
