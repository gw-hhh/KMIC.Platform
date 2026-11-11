using Kmic.BuildingBlocks.Application.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模型定稿时执行模型校验，再收集唯一约束的错误映射，最后删掉框架的临时注解
/// </summary>
/// <remarks>
/// 不能在 OnModelCreating 中校验：EF 在读取模块配置之前，会把值对象等类型按约定临时发现为实体，
/// 这些类型要到定稿阶段才被清理掉。本约定追加在最后，运行在 EF 的清理之后
/// </remarks>
internal sealed class ModelValidationConvention(string module, DataScopeObjectCatalog scopeObjects) : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var model = modelBuilder.Metadata;
        ModelValidator.Validate(model, module, scopeObjects);
        UniqueConstraintErrors.Collect(model);

        foreach (var entityType in model.GetEntityTypes())
        {
            entityType.RemoveAnnotation(ModelAnnotations.DataScope);
            entityType.RemoveAnnotation(ModelAnnotations.NoDataScope);
            foreach (var index in entityType.GetDeclaredIndexes())
            {
                index.RemoveAnnotation(ModelAnnotations.UniqueError);
            }
        }
    }
}
