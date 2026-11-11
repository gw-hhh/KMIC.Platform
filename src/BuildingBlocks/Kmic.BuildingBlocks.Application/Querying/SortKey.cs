using System.Linq.Expressions;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 一个排序键：列表项上的取值表达式和方向
/// </summary>
internal sealed record SortKey(LambdaExpression Selector, bool Descending);
