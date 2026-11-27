namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>软删除列名，供查询中通过 EF.Property 读取。</summary>
public static class SoftDeleteColumns
{
    public const string IsDeleted = nameof(IsDeleted);
    public const string DeletedAt = nameof(DeletedAt);
    public const string DeletedBy = nameof(DeletedBy);
}
