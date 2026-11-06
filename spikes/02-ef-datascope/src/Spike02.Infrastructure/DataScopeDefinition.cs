using System.Linq.Expressions;

namespace Spike02.Infrastructure;

internal sealed class DataScopeDefinition(string scopeObject)
{
    public const string AnnotationName = "Kmic:DataScope";

    public string ScopeObject { get; } = scopeObject;

    public LambdaExpression? Relation { get; set; }

    public LambdaExpression? Visibility { get; set; }
}
