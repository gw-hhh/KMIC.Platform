namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 软删除影子列的名称；回收站、引用已删除数据时投影 IsDeleted 供界面显示标记
/// </summary>
public static class SoftDeleteColumns
{
    /// <summary>
    /// 是否已删除
    /// </summary>
    public const string IsDeleted = nameof(IsDeleted);

    /// <summary>
    /// 删除时刻
    /// </summary>
    public const string DeletedAt = nameof(DeletedAt);

    /// <summary>
    /// 删除人
    /// </summary>
    public const string DeletedBy = nameof(DeletedBy);
}
