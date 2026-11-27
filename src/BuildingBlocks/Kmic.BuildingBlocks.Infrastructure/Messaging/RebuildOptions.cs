namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>读模型重建配置</summary>
public sealed class RebuildOptions
{
    /// <summary>单页从源端读取与单事务写入的条数</summary>
    public int PageSize { get; set; } = 500;

    /// <summary>收尾阶段允许直接置为终态的缺失行数阈值</summary>
    public int MaxMissingCount { get; set; } = 100;

    /// <summary>收尾阶段允许直接置为终态的缺失行数比例上限（与 MaxMissingCount 取较大值，超出后暂停等运维确认）</summary>
    public double MaxMissingRatio { get; set; } = 0.01;
}
