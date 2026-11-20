namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 容量保护对这次写入的处理方式
/// </summary>
internal enum AdmissionMode
{
    /// <summary>
    /// 软线、硬线都拒绝
    /// </summary>
    Enforce,

    /// <summary>
    /// 命令声明了软线下仍可执行：只有硬线拒绝
    /// </summary>
    ExemptFromSoftLine,

    /// <summary>
    /// 只在硬线下不写，也不报错：用于主事务不该因此回滚的通知类消息
    /// </summary>
    SkipWhenSuspended,
}
