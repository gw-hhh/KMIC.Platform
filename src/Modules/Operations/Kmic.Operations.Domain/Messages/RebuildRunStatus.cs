using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>
/// 读模型重建状态
/// </summary>
/// <remarks>
/// 编码与框架重建状态一一对应
/// </remarks>
public sealed class RebuildRunStatus : Enumeration<RebuildRunStatus>
{
    /// <summary>
    /// 排队或进行中
    /// </summary>
    public static readonly RebuildRunStatus Active = new("Active", "进行中");

    /// <summary>
    /// 源端缺失行数超限，等待运维确认
    /// </summary>
    public static readonly RebuildRunStatus AwaitingConfirmation = new("AwaitingConfirmation", "等待确认");

    /// <summary>
    /// 已完成
    /// </summary>
    public static readonly RebuildRunStatus Completed = new("Completed", "已完成");

    /// <summary>
    /// 已由运维放弃
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
