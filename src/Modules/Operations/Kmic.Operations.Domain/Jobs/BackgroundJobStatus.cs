using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>
/// 后台作业的状态
/// </summary>
/// <remarks>
/// 编码就是框架里作业状态枚举的成员名；领域层不引用应用层，所以单独定义，由应用层转换
/// </remarks>
public sealed class BackgroundJobStatus : Enumeration<BackgroundJobStatus>
{
    /// <summary>
    /// 排队中
    /// </summary>
    public static readonly BackgroundJobStatus Queued = new("Queued", "排队中");

    /// <summary>
    /// 运行中
    /// </summary>
    public static readonly BackgroundJobStatus Running = new("Running", "运行中");

    /// <summary>
    /// 成功
    /// </summary>
    public static readonly BackgroundJobStatus Succeeded = new("Succeeded", "成功");

    /// <summary>
    /// 失败
    /// </summary>
    public static readonly BackgroundJobStatus Failed = new("Failed", "失败");

    /// <summary>
    /// 已请求取消，执行器还没停
    /// </summary>
    public static readonly BackgroundJobStatus Cancelling = new("Cancelling", "取消中");

    /// <summary>
    /// 已取消
    /// </summary>
    public static readonly BackgroundJobStatus Cancelled = new("Cancelled", "已取消");

    private BackgroundJobStatus(string code, string name)
        : base(code, name)
    {
    }
}
