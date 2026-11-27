namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>声明后台作业元数据；必须配置 Permission 或 SystemOnly = true。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BackgroundJobAttribute(string name, string displayName) : Attribute
{
    public string Name { get; } = name;

    public string DisplayName { get; } = displayName;

    /// <summary>参数版本，从 1 开始。</summary>
    public int Version { get; init; } = 1;

    /// <summary>提交与执行时检查的权限点。</summary>
    public string Permission { get; init; } = "";

    public bool SystemOnly { get; init; }

    /// <summary>中断后是否从检查点续跑；仅在已正确使用检查点或重放无副作用时声明。</summary>
    public bool Resumable { get; init; }

    /// <summary>单用户并发上限（包含排队与运行中）。</summary>
    public int MaxConcurrentPerUser { get; init; } = 3;

    /// <summary>作业结束时是否记录审计；名称包含 Export 的作业必须开启。</summary>
    public bool Audited { get; init; }
}
