namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 依赖检查的标签。模块和平台模块的依赖（短信网关、邮件服务等）用同一个标签登记自己的检查，就会出现在依赖状态里
/// </summary>
public static class DependencyHealth
{
    /// <summary>
    /// 标签名
    /// </summary>
    public const string Tag = "dependencies";
}
