using Kmic.BuildingBlocks.Application.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kmic.ArchitectureTests.Support;

// 从模块设计时 EF 模型中收集带数据范围过滤的表（格式为 "schema"."表名"），不连接数据库
internal static class ScopedTables
{
    public static IReadOnlySet<string> All { get; } = Discover();

    private static HashSet<string> Discover()
    {
        var tables = new HashSet<string>(StringComparer.Ordinal);
        foreach (var factory in ProductAssemblies.Of(Layer.Infrastructure).SelectMany(a => a.Assembly.GetTypes()).Where(IsContextFactory))
        {
            var create = factory.GetInterfaces().Single(IsFactoryInterface).GetMethod(nameof(IDesignTimeDbContextFactory<>.CreateDbContext))!;
            using var context = (DbContext)create.Invoke(Activator.CreateInstance(factory, nonPublic: true), [Array.Empty<string>()])!;
            foreach (var entity in context.Model.GetEntityTypes())
            {
                if (entity.GetTableName() is { } table
                    && entity.GetDeclaredQueryFilters().Any(f => string.Equals(f.Key, QueryFilters.DataScope, StringComparison.Ordinal)))
                {
                    tables.Add($"\"{entity.GetSchema()}\".\"{table}\"");
                }
            }
        }

        return tables;
    }

    private static bool IsContextFactory(Type type) => type is { IsClass: true, IsAbstract: false } && type.GetInterfaces().Any(IsFactoryInterface);

    private static bool IsFactoryInterface(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>);
}
