namespace Kmic.BuildingBlocks.Testing;

internal static class TestContainerNames
{
    // Docker Desktop 按此标签将同一组容器归类显示
    public const string GroupLabel = "com.docker.compose.project";

    public const string Group = "kmic-test";

    public static string For(string purpose) => $"{Group}-{purpose}-{Guid.CreateVersion7().ToString("N")[^8..]}";
}
