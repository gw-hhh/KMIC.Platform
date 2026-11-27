using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal static class UniqueConstraintErrors
{
    // 避免存入模型注解而污染迁移快照；由于 EF Core 在进程生命周期内仅构建一次模型且仅收集一次，故缓存于静态字段中
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
