namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

// 框架在建模期间临时写在实体和索引上的标记，校验完就删掉，不会进迁移快照
internal static class ModelAnnotations
{
    public const string DataScope = "Kmic:DataScope";

    public const string NoDataScope = "Kmic:NoDataScope";

    public const string UniqueError = "Kmic:UniqueError";
}
