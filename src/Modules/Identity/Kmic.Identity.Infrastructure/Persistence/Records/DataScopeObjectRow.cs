namespace Kmic.Identity.Infrastructure.Persistence.Records;

/// <summary>
/// 范围对象目录表的一行：代码里声明的范围对象同步到这里，只由目录同步与清理写
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
