namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

// 框架在构建模型期间写在实体与索引上的声明；定稿时校验、收集后删除，不进入最终模型与迁移快照
internal static class ModelAnnotations
{
    public const string DataScope = "Kmic:DataScope";

    public const string NoDataScope = "Kmic:NoDataScope";

    public const string UniqueError = "Kmic:UniqueError";
}
