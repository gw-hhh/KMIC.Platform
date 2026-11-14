namespace Kmic.Identity.Infrastructure.Persistence.Records;

/// <summary>
/// 数据范围对象目录，仅由同步与清理更新
/// </summary>
internal sealed class DataScopeObjectRow
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 编码，唯一
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// 所属模块
    /// </summary>
    public required string Module { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// 代码里已经不存在
    /// </summary>
    public bool IsRetired { get; set; }

    /// <summary>
    /// 退役的时间
    /// </summary>
    public DateTimeOffset? RetiredAt { get; set; }
}
