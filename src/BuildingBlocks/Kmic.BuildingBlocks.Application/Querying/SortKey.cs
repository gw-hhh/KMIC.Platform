using System.Linq.Expressions;

namespace Kmic.BuildingBlocks.Application.Querying;

internal sealed record SortKey(LambdaExpression Selector, bool Descending);
