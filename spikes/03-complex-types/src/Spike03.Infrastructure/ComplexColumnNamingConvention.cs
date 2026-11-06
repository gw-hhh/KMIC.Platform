using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Spike03.Infrastructure;

/// <summary>
/// 复杂类型的列名为“属性名 + 成员名”，不带下划线（05-03 第 3.1 节：AmountValue、AddressCity）；
/// 存为 JSON 的复杂类型不是列，不处理
/// </summary>
public sealed class ComplexColumnNamingConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var complexProperty in entityType.GetComplexProperties())
            {
                Rename(complexProperty, complexProperty.Name);
            }
        }
    }

    private static void Rename(IConventionComplexProperty complexProperty, string prefix)
    {
        if (complexProperty.IsCollection || complexProperty.ComplexType.IsMappedToJson())
        {
            return;
        }

        foreach (var property in complexProperty.ComplexType.GetProperties())
        {
            property.Builder.HasColumnName(prefix + property.Name);
        }

        foreach (var nested in complexProperty.ComplexType.GetComplexProperties())
        {
            Rename(nested, prefix + nested.Name);
        }
    }
}
