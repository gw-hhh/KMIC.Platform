namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 审计列名
/// </summary>
/// <remarks>
/// 查询里用 EF.Property 读取，如 EF.Property&lt;DateTimeOffset&gt;(x, AuditColumns.CreatedAt)
/// </remarks>
public static class AuditColumns
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public const string CreatedAt = nameof(CreatedAt);

    /// <summary>
    /// 创建人
    /// </summary>
    public const string CreatedBy = nameof(CreatedBy);

    /// <summary>
    /// 修改时间
    /// </summary>
    public const string UpdatedAt = nameof(UpdatedAt);

    /// <summary>
    /// 修改人
    /// </summary>
    public const string UpdatedBy = nameof(UpdatedBy);
}
