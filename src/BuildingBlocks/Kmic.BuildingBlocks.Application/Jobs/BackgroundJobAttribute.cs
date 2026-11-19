namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 后台作业的声明
/// </summary>
/// <remarks>
/// Permission 和 SystemOnly 必须写一个
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BackgroundJobAttribute(string name, string displayName) : Attribute
{
    /// <summary>
    /// 作业名称
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 参数版本，从 1 开始
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// 提交与执行时检查的权限点
    /// </summary>
    public string Permission { get; init; } = "";

    /// <summary>
    /// 是否只能由系统身份提交
    /// </summary>
    public bool SystemOnly { get; init; }

    /// <summary>
    /// 中断后是否从检查点续跑
    /// </summary>
    /// <remarks>
    /// 只有正确用了检查点或重做没有副作用才能声明
    /// </remarks>
    public bool Resumable { get; init; }

    /// <summary>
    /// 每个用户同时排队或运行的上限
    /// </summary>
    public int MaxConcurrentPerUser { get; init; } = 3;

    /// <summary>
    /// 作业结束时是否写审计事件
    /// </summary>
    /// <remarks>
    /// 名称里有 Export 的作业必须审计
    /// </remarks>
    public bool Audited { get; init; }
}
