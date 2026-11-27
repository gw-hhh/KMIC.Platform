using System.Linq.Expressions;
using System.Reflection;

namespace Kmic.BuildingBlocks.Application.Querying;

// 将排序与筛选操作下推至投影之前执行
internal static class ProjectionOrdering
{
    private static readonly MethodInfo _select = Queryable("Select", lambdaParameters: 1);
    private static readonly MethodInfo _where = Queryable(nameof(System.Linq.Queryable.Where), lambdaParameters: 1);
    private static readonly MethodInfo _orderBy = Queryable(nameof(System.Linq.Queryable.OrderBy));
    private static readonly MethodInfo _orderByDescending = Queryable(nameof(System.Linq.Queryable.OrderByDescending));
    private static readonly MethodInfo _thenBy = Queryable(nameof(System.Linq.Queryable.ThenBy));
    private static readonly MethodInfo _thenByDescending = Queryable(nameof(System.Linq.Queryable.ThenByDescending));

    public static IQueryable<T> Order<T>(IQueryable<T> query, IReadOnlyList<SortKey> keys)
    {
        if (query.Expression is MethodCallExpression { Method: { IsGenericMethod: true } method, Arguments: [var source, UnaryExpression { Operand: LambdaExpression { Parameters.Count: 1 } projection } quoted] }
            && method.GetGenericMethodDefinition() == _select)
        {
            var rebased = keys.Select(k => (Selector: Rebase(k.Selector, projection), k.Descending));
            var ordered = OrderExpression(source, projection.Parameters[0].Type, rebased);
            return query.Provider.CreateQuery<T>(Expression.Call(method, ordered, quoted));
        }

        return query.Provider.CreateQuery<T>(OrderExpression(query.Expression, typeof(T), keys.Select(k => (k.Selector, k.Descending))));
    }

    // 将基于列表项成员的筛选条件还原为投影前的源表达式，实现先筛选后投影
    public static IQueryable<T> Where<T>(IQueryable<T> query, Expression<Func<T, bool>> predicate)
    {
        if (query.Expression is MethodCallExpression { Method: { IsGenericMethod: true } method, Arguments: [var source, UnaryExpression { Operand: LambdaExpression { Parameters.Count: 1 } projection } quoted] }
            && method.GetGenericMethodDefinition() == _select)
        {
            var filtered = Expression.Call(_where.MakeGenericMethod(projection.Parameters[0].Type), source, Expression.Quote(Rebase(predicate, projection)));
            return query.Provider.CreateQuery<T>(Expression.Call(method, filtered, quoted));
        }

        return query.Where(predicate);
    }

    private static Expression OrderExpression(Expression source, Type itemType, IEnumerable<(LambdaExpression Selector, bool Descending)> keys)
    {
        var current = source;
        var first = true;
        foreach (var (selector, descending) in keys)
        {
            var method = (first, descending) switch
            {
                (true, false) => _orderBy,
                (true, true) => _orderByDescending,
                (false, false) => _thenBy,
                (false, true) => _thenByDescending,
            };

            current = Expression.Call(method.MakeGenericMethod(itemType, selector.ReturnType), current, Expression.Quote(selector));
            first = false;
        }

        return current;
    }

    // 排序键基于列表项成员取值，将其替换为投影表达式中对应成员的源表达式
    private static LambdaExpression Rebase(LambdaExpression selector, LambdaExpression projection) =>
        Expression.Lambda(new MemberInliner(selector.Parameters[0], projection.Body).Visit(selector.Body), projection.Parameters);

    private static MethodInfo Queryable(string name, int lambdaParameters = 1) =>
        typeof(System.Linq.Queryable).GetMethods().Single(m =>
            string.Equals(m.Name, name, StringComparison.Ordinal)
            && m.GetParameters() is [_, { ParameterType: var lambda }]
            && lambda.GetGenericArguments()[0].GetGenericArguments().Length == lambdaParameters + 1);

    private sealed class MemberInliner(ParameterExpression parameter, Expression replacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == parameter ? replacement : node;

        protected override Expression VisitMember(MemberExpression node)
        {
            var inner = Visit(node.Expression);
            return inner is null ? node : Resolve(inner, node.Member) ?? node.Update(inner);
        }

        // 成员访问针对新建对象时，直接提取构造该成员的原始表达式
        private static Expression? Resolve(Expression source, MemberInfo member) => source switch
        {
            MemberInitExpression init =>
                init.Bindings.OfType<MemberAssignment>().FirstOrDefault(b => string.Equals(b.Member.Name, member.Name, StringComparison.Ordinal))?.Expression
                ?? Resolve(init.NewExpression, member),
            NewExpression created => ArgumentFor(created, member),
            _ => null,
        };

        // 匿名类型自带成员映射；常规构造函数（如记录主构造函数）按参数名与成员名忽略大小写匹配
        private static Expression? ArgumentFor(NewExpression created, MemberInfo member)
        {
            if (created.Members is { } members)
            {
                var index = members.ToList().FindIndex(m => string.Equals(m.Name, member.Name, StringComparison.Ordinal));
                return index >= 0 ? created.Arguments[index] : null;
            }

            var parameters = created.Constructor?.GetParameters() ?? [];
            var position = Array.FindIndex(parameters, p => string.Equals(p.Name, member.Name, StringComparison.OrdinalIgnoreCase));
            return position >= 0 ? created.Arguments[position] : null;
        }
    }
}
