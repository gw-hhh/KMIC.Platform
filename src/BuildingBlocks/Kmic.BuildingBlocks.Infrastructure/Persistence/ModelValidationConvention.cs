using Kmic.BuildingBlocks.Application.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模型校验约定
/// </summary>
/// <remarks>
/// 不在 OnModelCreating 里校验：那时值对象这类类型还被 EF 临时当成实体，要到 ModelFinalizing 才清理掉。
/// 这个约定最后加入，在 EF 清理之后运行
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
