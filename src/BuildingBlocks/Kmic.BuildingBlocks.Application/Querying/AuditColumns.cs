namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>审计列名，供查询中通过 EF.Property 读取。</summary>
public static class AuditColumns
{
    public const string CreatedAt = nameof(CreatedAt);
    public const string CreatedBy = nameof(CreatedBy);
    public const string UpdatedAt = nameof(UpdatedAt);
    public const string UpdatedBy = nameof(UpdatedBy);
}
