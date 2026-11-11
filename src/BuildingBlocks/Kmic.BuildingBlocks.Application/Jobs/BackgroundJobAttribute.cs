namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 后台作业的声明
/// </summary>
/// <remarks>
/// Permission 和 SystemOnly 必须写一个：用户提交的作业提交与执行时都检查权限点，系统专用的只能由系统身份提交
/// </remarks>
/// <param name="name">作业名称，以本模块名开头，如 Sample.ImportSamples</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BackgroundJobAttribute(string name) : Attribute
{
    /// <summary>
    /// 作业名称
    /// </summary>
    public string Name { get; } = name;

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
    /// 不声明的作业中断后标记失败，要用户重新提交。只有处理器正确使用了检查点、或重做没有副作用时才能声明
    /// </remarks>
    public bool Resumable { get; init; }

    /// <summary>
    /// 每个用户同时排队或运行的上限
    /// </summary>
    public int MaxConcurrentPerUser { get; init; } = 3;

    /// <summary>
    /// 作业结束时是否写一条审计事件
    /// </summary>
    /// <remarks>
    /// 名称里有 Export 的作业必须审计。作业里每一批发送的命令按各自的 [Audited] 另行审计
    /// </remarks>
    public bool Audited { get; init; }
}
