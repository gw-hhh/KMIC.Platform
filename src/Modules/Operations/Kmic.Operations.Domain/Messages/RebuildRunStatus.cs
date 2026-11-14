using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>
/// 读模型重建一轮的状态
/// </summary>
/// <remarks>
/// 编码和框架里重建轮次的状态一一对应，领域层不引用应用层，所以单独定义，由应用层转换
/// </remarks>
public sealed class RebuildRunStatus : Enumeration<RebuildRunStatus>
{
    /// <summary>
    /// 排队或进行中
    /// </summary>
    public static readonly RebuildRunStatus Active = new("Active", "进行中");

    /// <summary>
    /// 源端找不到的行太多，暂停等运维确认
    /// </summary>
    public static readonly RebuildRunStatus AwaitingConfirmation = new("AwaitingConfirmation", "等待确认");

    /// <summary>
    /// 已完成
    /// </summary>
    public static readonly RebuildRunStatus Completed = new("Completed", "已完成");

    /// <summary>
    /// 运维放弃了这一轮
    /// </summary>
    public static readonly RebuildRunStatus Abandoned = new("Abandoned", "已放弃");

    /// <summary>
    /// 作业失败或被取消
    /// </summary>
    public static readonly RebuildRunStatus Failed = new("Failed", "已失败");

    private RebuildRunStatus(string code, string name)
        : base(code, name)
    {
    }
}
