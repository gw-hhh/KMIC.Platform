namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 软删除列名
/// </summary>
public static class SoftDeleteColumns
{
    /// <summary>
    /// 是否已删除
    /// </summary>
    public const string IsDeleted = nameof(IsDeleted);

    /// <summary>
    /// 删除时间
    /// </summary>
    public const string DeletedAt = nameof(DeletedAt);

    /// <summary>
    /// 删除人
    /// </summary>
    public const string DeletedBy = nameof(DeletedBy);
}
