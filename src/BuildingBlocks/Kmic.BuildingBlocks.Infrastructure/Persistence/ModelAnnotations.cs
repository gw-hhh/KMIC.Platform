namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

// 框架在模型构建期间临时附加于实体和索引的注解标记，校验完成后立即移除，不会进入迁移快照
internal static class ModelAnnotations
{
    public const string DataScope = "Kmic:DataScope";

    public const string NoDataScope = "Kmic:NoDataScope";

    public const string UniqueError = "Kmic:UniqueError";
}
