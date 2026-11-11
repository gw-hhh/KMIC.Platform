namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 审计影子列的名称，查询中用 EF.Property 读取，如 EF.Property&lt;DateTimeOffset&gt;(x, AuditColumns.CreatedAt)
/// </summary>
public static class AuditColumns
{
    /// <summary>
    /// 创建时刻
    /// </summary>
    public const string CreatedAt = nameof(CreatedAt);

    /// <summary>
    /// 创建人；模拟身份时是实际操作人
    /// </summary>
    public const string CreatedBy = nameof(CreatedBy);

    /// <summary>
    /// 最后修改时刻，新增后未修改过为空
    /// </summary>
    public const string UpdatedAt = nameof(UpdatedAt);

    /// <summary>
    /// 最后修改人
    /// </summary>
    public const string UpdatedBy = nameof(UpdatedBy);
}
