using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 唯一约束对应的错误
/// </summary>
/// <remarks>
/// 建模时从 MapsToError 收集，异常转换时按 schema 和约束名查找
/// </remarks>
internal static class UniqueConstraintErrors
{
    // 不放在模型注解里，否则会写进迁移快照。EF 在进程里只建一次模型，收集也只有一次，所以存在静态字段里
    private static readonly ConcurrentDictionary<(string Schema, string Constraint), Error> _errors = new();

    public static Error? Find(string? schema, string? constraint) =>
        schema is not null && constraint is not null && _errors.TryGetValue((schema, constraint), out var error) ? error : null;

    public static void Collect(IReadOnlyModel model)
    {
        foreach (var index in model.GetEntityTypes().SelectMany(e => e.GetDeclaredIndexes()))
        {
            if (index.FindAnnotation(ModelAnnotations.UniqueError)?.Value is Error error
                && index.DeclaringEntityType.GetSchema() is { } schema
                && index.GetDatabaseName() is { } name)
            {
                _errors[(schema, name)] = error;
            }
        }
    }
}
