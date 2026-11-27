namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>依赖健康检查标签，带此标签的检查项会汇总到依赖状态端点。</summary>
public static class DependencyHealth
{
    public const string Tag = "dependencies";
}
