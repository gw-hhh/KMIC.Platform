using Kmic.BuildingBlocks.Application.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

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
