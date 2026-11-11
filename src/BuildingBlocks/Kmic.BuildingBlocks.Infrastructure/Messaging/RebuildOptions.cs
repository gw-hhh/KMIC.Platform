namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 读模型重建配置
/// </summary>
public sealed class RebuildOptions
{
    /// <summary>
    /// 每页从源端读取的条数，也是每个事务写入的条数
    /// </summary>
    public int PageSize { get; set; } = 500;

    /// <summary>
    /// 收尾时源端找不到的行数不超过这个数，就直接置为终态
    /// </summary>
    public int MaxMissingCount { get; set; } = 100;

    /// <summary>
    /// 收尾时源端找不到的行数不超过读模型总行数乘以这个比例，也直接置为终态
    /// </summary>
    /// <remarks>
    /// 和 MaxMissingCount 取大的那个，超过就暂停等运维确认
    /// </remarks>
    public double MaxMissingRatio { get; set; } = 0.01;
}
