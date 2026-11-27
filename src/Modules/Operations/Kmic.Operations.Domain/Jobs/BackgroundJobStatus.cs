using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>后台作业状态（编码与框架作业状态成员名一致）。</summary>
public sealed class BackgroundJobStatus : Enumeration<BackgroundJobStatus>
{
    public static readonly BackgroundJobStatus Queued = new("Queued", "排队中");

    public static readonly BackgroundJobStatus Running = new("Running", "运行中");

    public static readonly BackgroundJobStatus Succeeded = new("Succeeded", "成功");

    public static readonly BackgroundJobStatus Failed = new("Failed", "失败");

    /// <summary>已请求取消，执行器尚未停止。</summary>
    public static readonly BackgroundJobStatus Cancelling = new("Cancelling", "取消中");

    public static readonly BackgroundJobStatus Cancelled = new("Cancelled", "已取消");

    private BackgroundJobStatus(string code, string name)
        : base(code, name)
    {
    }
}
