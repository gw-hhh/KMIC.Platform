using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 唯一约束到模块错误的映射：构建模型时从 MapsToError 收集，异常转换按 schema 与约束名查找
/// </summary>
/// <remarks>
/// 声明不留在模型注解里，否则会写进迁移快照（Error 不能生成为代码）。EF 按上下文类型在进程内缓存模型，
/// 收集同样只发生一次，所以映射也放在进程级
/// </remarks>
internal static class UniqueConstraintErrors
{
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
